using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	public static class AreaEditable
	{
		static AreaEditable()
		{
			ThisMod.Harmony().Patch(AccessTools.Constructor(typeof(Dialog_ManageAreas), new Type[] { typeof(Map) }),
				null, new HarmonyMethod(typeof(Dialog_ManageAreas_Patch), "Postfix"));
			ThisMod.Harmony().Patch(AccessTools.Property(typeof(Area_Allowed), nameof(Area_Allowed.ListPriority)).GetGetMethod(false),
				null, new HarmonyMethod(typeof(AreaOrder), nameof(AreaOrder.ListPriority_Postfix)));
		}
	}

	[StaticConstructorOnStartup]
	public static class TexButton
	{
		public static readonly Texture2D ReorderUp = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderUp", true);
		public static readonly Texture2D ReorderDown = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderDown", true);
		public static readonly Texture2D Copy = ContentFinder<Texture2D>.Get("UI/Buttons/Copy");
		public static readonly Texture2D Paste = ContentFinder<Texture2D>.Get("UI/Buttons/Paste");
	}

	[HarmonyPatch(typeof(Dialog_ManageAreas))]
	[HarmonyPatch("InitialSize", PropertyMethod.Getter)]
	static class InitialSize_Patch
	{
		public static void Postfix(ref Vector2 __result)
		{
			AreaRowPatch.copiedArea = null;
			__result.x += 120;//about 4 icon widths?
		}
	}

	//[HarmonyPatch(typeof(Dialog_ManageAreas), "Dialog_ManageAreas")]
	static class Dialog_ManageAreas_Patch
	{
		public static void Postfix()
		{
			AreaRowPatch.copiedArea = null;
		}
	}


	//private static void DoAreaRow(Rect rect, Area area)
	[HarmonyPatch(typeof(Dialog_ManageAreas))]
	[HarmonyPatch("DoAreaRow")]
	static class AreaRowPatch
	{
		public static Area copiedArea = null;
		
		//Insert FilterForUrgentHediffs when counting needed medicine
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo IconInfo = AccessTools.Method(
				typeof(WidgetRow), nameof(WidgetRow.Icon));
			MethodInfo EndGroupInfo = AccessTools.Method(
				typeof(GUI), nameof(GUI.EndGroup));

			MethodInfo DoOrderButtonInfo = AccessTools.Method(
				typeof(AreaRowPatch), nameof(DoOrderButton));
			MethodInfo DoButtonIconInfo = AccessTools.Method(
				typeof(AreaRowPatch), nameof(DoButtonIcon));
			MethodInfo DoCopyPasteInfo = AccessTools.Method(
				typeof(AreaRowPatch), nameof(CopyPasteAreaRow));

			foreach (CodeInstruction instruction in instructions)
			{

				//IL_0055: callvirt instance valuetype[UnityEngine]UnityEngine.Rect Verse.WidgetRow::Icon(class [UnityEngine] UnityEngine.Texture2D, string)
				if (instruction.opcode == OpCodes.Callvirt && instruction.operand == IconInfo)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1); //Area
					yield return new CodeInstruction(OpCodes.Callvirt, DoButtonIconInfo); //WidgetRow
					yield return new CodeInstruction(OpCodes.Ldnull); //popped off
					continue;
				}

				if (instruction.opcode == OpCodes.Call && instruction.operand == EndGroupInfo)
				{
					yield return new CodeInstruction(OpCodes.Ldloc_0) { labels = instruction.labels }; //WidgetRow
					instruction.labels = new List<Label>();
					yield return new CodeInstruction(OpCodes.Ldarg_1); //Area
					yield return new CodeInstruction(OpCodes.Call, DoOrderButtonInfo);  //DoOrderButton(widgetRow, area)
				}

				yield return instruction;

				if (instruction.opcode == OpCodes.Stloc_0)
				{
					yield return new CodeInstruction(OpCodes.Ldloc_0); //widgetRow
					yield return new CodeInstruction(OpCodes.Ldarg_1); //Area
					yield return new CodeInstruction(OpCodes.Call, DoCopyPasteInfo);
				}
			}
		}

		public static void DoOrderButton(WidgetRow widgetRow, Area areaBase)
		{
			if (!(areaBase is Area_Allowed area)) return;
			List<Area> areas = area.Map.areaManager.AllAreas.FindAll(a => a is Area_Allowed);
			int index = areas.IndexOf(area);

			if (index > 0 && widgetRow.ButtonIcon(TexButton.ReorderUp))
			{
				Area other = areas[index - 1];
				area.Map.GetComponent<MapComponent_AreaOrder>().Swap(area, other);
			}
			if (index < areas.Count - 1 && widgetRow.ButtonIcon(TexButton.ReorderDown))
			{
				Area other = areas[index + 1];
				area.Map.GetComponent<MapComponent_AreaOrder>().Swap(area, other);
			}
		}

		public static void DoButtonIcon(WidgetRow widgetRow, Texture2D tex, string tooltip, Area area)
		{
			if (widgetRow.ButtonIcon(tex, tooltip))
			{
				if (area is Area_Allowed aa)
				{
					Find.WindowStack.Add(new Dialog_RecolorArea(aa));
					//TODO: better dialog
				}
			}
		}
		
		public static void CopyPasteAreaRow(WidgetRow widgetRow, Area area)
		{
			//Gap doesn't work if it's the first thing. So dumb. Increment is private. So dumb.
			//Have to hack in the method call instead AEH.
			MethodInfo IncrementPositionInfo = AccessTools.Method(typeof(WidgetRow), "IncrementPosition");
			float gapWidth = WidgetRow.DefaultGap + WidgetRow.IconSize;
			if (copiedArea == area)
			{
				IncrementPositionInfo.Invoke(widgetRow, new object[] { gapWidth * 2});
				return;
			}

			if (widgetRow.ButtonIcon(TexButton.Copy))
				copiedArea = area;

			if (copiedArea == null)
				IncrementPositionInfo.Invoke(widgetRow, new object[] { gapWidth });
			else if (widgetRow.ButtonIcon(TexButton.Paste))
					PasteArea(copiedArea, area);
				
		}

		public static void PasteArea(Area copy, Area paste)
		{
			if (copy != null)
				foreach(IntVec3 cell in copy.ActiveCells)
					paste[cell] = true;
		}

		
		public class Dialog_RecolorArea : Dialog_Rename
		{
			private Area_Allowed area;

			public Dialog_RecolorArea(Area_Allowed area)
			{
				this.area = area;
				this.curName = ColorUtility.ToHtmlStringRGB(area.Color).ToUpper();
			}

			protected override AcceptanceReport NameIsValid(string name)
			{
				AcceptanceReport result = base.NameIsValid(name);
				if (!result.Accepted)
				{
					return result;
				}
				if (ColorUtility.TryParseHtmlString(name, out Color c))
				{
					return "TD.HexColorValuesOnly".Translate();
				}
				return true;
			}

			protected override void SetName(string name)
			{
				Color newColor = new Color();
				ColorUtility.TryParseHtmlString("#" + name, out newColor);

				area.SetColor(newColor);
			}
		}

		public static void SetColor(this Area_Allowed area, Color color)
		{
			FieldInfo colorInfo = AccessTools.Field(typeof(Area_Allowed), "colorInt");
			colorInfo.SetValue(area, color);

			FieldInfo colorTextureInfo = AccessTools.Field(typeof(Area_Allowed), "colorTextureInt");
			colorTextureInfo.SetValue(area, null);

			FieldInfo drawerInfo = AccessTools.Field(typeof(Area_Allowed), "drawer");
			drawerInfo.SetValue(area, null);
		}
	}

	
	[HarmonyPatch(typeof(AreaManager))]
	[HarmonyPatch("TryMakeNewAllowed")]
	static class TryMakeNewAllowed_Patch
	{
		public static void Postfix(bool __result,	Area area, AreaManager __instance)
		{
			if (__result)
				__instance.map.GetComponent<MapComponent_AreaOrder>()?.Notify_Added(area);
		}
	}
	
	[HarmonyPatch(typeof(AreaManager))]
	[HarmonyPatch("NotifyEveryoneAreaRemoved")]
	static class NotifyEveryoneAreaRemoved_Patch
	{
		public static void Postfix(Area area, AreaManager __instance)
		{
			__instance.map.GetComponent<MapComponent_AreaOrder>()?.Notify_Removed(area);
		}
	}


	//public class Area_Allowed : ListPriority : get
	//HarmonyPatch AccessTools.Property(typeof(Area_Allowed), nameof(Area_Allowed.ListPriority)).GetGetMethod(false)
	//[HarmonyPatch(typeof(Area_Allowed), "get_ListPriority")]
	class AreaOrder
	{
		public static void ListPriority_Postfix(Area_Allowed __instance, ref int __result)
		{
			__result -= __instance.Map.GetComponent<MapComponent_AreaOrder>()?.AdjustFor(__instance) ?? 0;
		}
	}


	public class MapComponent_AreaOrder : MapComponent
	{
		public Dictionary<int, int> areaIndex;

		public MapComponent_AreaOrder(Map map) : base(map)
		{
			InitIndex();
		}

		public void Swap(Area_Allowed a, Area b)
		{
			int temp = areaIndex[a.ID];
			areaIndex[a.ID] = areaIndex[b.ID];
			areaIndex[b.ID] = temp;
			SortMap();
		}
		public void SortMap()
		{
			AccessTools.Method(typeof(AreaManager), "SortAreas").Invoke(map.areaManager, new object[] { });
		}

		public void InitIndex()
		{
			areaIndex = new Dictionary<int, int>();
			int index = 0;
			foreach (Area area in map.areaManager.AllAreas)
			{
				if (area is Area_Allowed)
					areaIndex[area.ID] = index++;
			}
		}
		public void Notify_Added(Area areaBase)
		{
			if (!(areaBase is Area_Allowed area)) return;
			int count = map.areaManager.AllAreas.FindAll(a => a is Area_Allowed aa).Count - 1;
			areaIndex[area.ID] = count;

			//Would be better to transpile earlier and let the normal sort happen, but this works
			SortMap();
		}
		public void Notify_Removed(Area areaBase)
		{
			if (!(areaBase is Area_Allowed area)) return;
			int index = areaIndex[area.ID];
			areaIndex.Remove(area.ID);

			List<int> keys = new List<int>(areaIndex.Keys);
			foreach (int key in keys)
			{
				if (areaIndex[key] > index)
					areaIndex[key]--;
			}
		}
		public int AdjustFor(Area_Allowed area)
		{
			return areaIndex.GetValueSafe(area.ID);
		}
		public override void ExposeData()
		{
			Scribe_Collections.Look(ref areaIndex, "areaIndex");
			if (areaIndex == null || areaIndex.Count == 0)
				InitIndex();
		}
	}

	[HarmonyPatch(typeof(AreaAllowedGUI), "DoAreaSelector")]
	public static class DoAreaSelector_Patch
	{
		//private static void DoAreaSelector(Rect rect, Pawn p, Area area)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo WidgetLabelInfo = AccessTools.Method(typeof(Widgets), "Label", new Type[] { typeof(Rect), typeof(string)});

			MethodInfo SetGUIColorInfo = AccessTools.Method(typeof(DoAreaSelector_Patch), nameof(SetGUIColor));
			MethodInfo SetGUIColorWhiteInfo = AccessTools.Method(typeof(DoAreaSelector_Patch), nameof(SetGUIColorWhite));

			foreach (CodeInstruction i in instructions)
			{
				if(i.opcode == OpCodes.Call && i.operand == WidgetLabelInfo)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_2);
					yield return new CodeInstruction(OpCodes.Call, SetGUIColorInfo);
				}
				yield return i;
				if (i.opcode == OpCodes.Call && i.operand == WidgetLabelInfo)
				{
					yield return new CodeInstruction(OpCodes.Call, SetGUIColorWhiteInfo);
				}
			}
		}

		public static void SetGUIColor(Area area)
		{
			GUI.color = area?.Color.grayscale > 0.55 ? Color.black : Color.white;
		}

		public static void SetGUIColorWhite()
		{
			GUI.color = Color.white;
		}
	}

	[HarmonyPatch(typeof(WidgetRow), "FillableBar")]
	public static class FillableBar_Patch
	{
		//public Rect FillableBar(float width, float height, float fillPct, string label, Texture2D fillTex, Texture2D bgTex = null)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo WidgetLabelInfo = AccessTools.Method(typeof(Widgets), "Label", new Type[] { typeof(Rect), typeof(string) });

			MethodInfo SetGUIColorInfo = AccessTools.Method(typeof(FillableBar_Patch), nameof(SetGUIColor));
			MethodInfo SetGUIColorWhiteInfo = AccessTools.Method(typeof(FillableBar_Patch), nameof(SetGUIColorWhite));

			foreach (CodeInstruction i in instructions)
			{
				if (i.opcode == OpCodes.Call && i.operand == WidgetLabelInfo)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//fillTex
					yield return new CodeInstruction(OpCodes.Call, SetGUIColorInfo);
				}
				yield return i;
				if (i.opcode == OpCodes.Call && i.operand == WidgetLabelInfo)
				{
					yield return new CodeInstruction(OpCodes.Call, SetGUIColorWhiteInfo);
				}
			}
		}

		public static void SetGUIColor(Texture2D tex)
		{
			GUI.color = tex.GetPixel(0,0).grayscale > 0.55 ? Color.black : Color.white;
		}

		public static void SetGUIColorWhite()
		{
			GUI.color = Color.white;
		}
	}
}