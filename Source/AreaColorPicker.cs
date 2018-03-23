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
	public static class TexButton
	{
		public static readonly Texture2D ReorderUp = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderUp", true);
		public static readonly Texture2D ReorderDown = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderDown", true);
	}


	//private static void DoAreaRow(Rect rect, Area area)
	[HarmonyPatch(typeof(Dialog_ManageAreas))]
	[HarmonyPatch("DoAreaRow")]
	static class AreaRowPatch
	{
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

			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call && instruction.operand == EndGroupInfo)
				{
					yield return new CodeInstruction(OpCodes.Ldloc_0) { labels = instruction.labels }; //WidgetRow
					instruction.labels = new List<Label>();
					yield return new CodeInstruction(OpCodes.Ldarg_1); //Area
					yield return new CodeInstruction(OpCodes.Call, DoOrderButtonInfo);  //DoOrderButton(widgetRow, area)
				}

				//IL_0055: callvirt instance valuetype[UnityEngine]UnityEngine.Rect Verse.WidgetRow::Icon(class [UnityEngine] UnityEngine.Texture2D, string)
				if (instruction.opcode == OpCodes.Callvirt && instruction.operand == IconInfo)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1); //Area
					yield return new CodeInstruction(OpCodes.Callvirt, DoButtonIconInfo); //WidgetRow
					yield return new CodeInstruction(OpCodes.Ldnull); //popped off
					continue;
				}
				yield return instruction;
			}
		}

		public static void DoOrderButton(WidgetRow widgetRow, Area areaBase)
		{
			if (!(areaBase is Area_Allowed area)) return;
			List<Area> areas = area.Map.areaManager.AllAreas.FindAll(a => a is Area_Allowed aa && aa.mode == area.mode);
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
			if(widgetRow.ButtonIcon(tex, tooltip))
			{
				if (area is Area_Allowed aa)
				{
					Find.WindowStack.Add(new Dialog_RecolorArea(aa));	
					//TODO: better dialog
				}
			}
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
					return "Hex color values only";
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
	class AreaOrder
	{
		public static void ListPriority_Postfix(Area_Allowed __instance, ref int __result)
		{
			__result -= __instance.Map.GetComponent<MapComponent_AreaOrder>()?.AdjustFor(__instance) ?? 0;
		}
	}


	public class MapComponent_AreaOrder : MapComponent
	{
		public Dictionary<int, int> humanIndex;
		public Dictionary<int, int> animalIndex;

		public MapComponent_AreaOrder(Map map) : base(map)
		{
			InitIndexH();
			InitIndexA();
		}

		public void Swap(Area_Allowed a, Area b)
		{
			Dictionary<int, int> areaIndex = a.mode == AllowedAreaMode.Humanlike ? humanIndex : animalIndex;
			int temp = areaIndex[a.ID];
			areaIndex[a.ID] = areaIndex[b.ID];
			areaIndex[b.ID] = temp;
			SortMap();
		}
		public void SortMap()
		{
			AccessTools.Method(typeof(AreaManager), "SortAreas").Invoke(map.areaManager, new object[] { });
		}

		public void InitIndexH()
		{
			humanIndex = new Dictionary<int, int>();
			int index = 0;
			foreach (Area a in map.areaManager.AllAreas)
			{
				if (!(a is Area_Allowed area) || area.mode != AllowedAreaMode.Humanlike) continue;
				humanIndex[a.ID] = index++;
			}
		}

		public void InitIndexA()
		{
			animalIndex = new Dictionary<int, int>();
			int index = 0;
			foreach (Area a in map.areaManager.AllAreas)
			{
				if (!(a is Area_Allowed area) || area.mode != AllowedAreaMode.Animal) continue;
				animalIndex[a.ID] = index++;
			}
		}
		public void Notify_Added(Area areaBase)
		{
			if (!(areaBase is Area_Allowed area)) return;
			int count = map.areaManager.AllAreas.FindAll(a => a is Area_Allowed aa && aa.mode == area.mode).Count - 1;
			Dictionary<int, int> areaIndex = area.mode == AllowedAreaMode.Humanlike ? humanIndex : animalIndex;
			areaIndex[area.ID] = count;

			//Would be better to transpile earlier and let the normal sort happen, but this works
			SortMap();
		}
		public void Notify_Removed(Area areaBase)
		{
			if (!(areaBase is Area_Allowed area)) return;
			Dictionary<int, int> areaIndex = area.mode == AllowedAreaMode.Humanlike ? humanIndex : animalIndex;
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
			return (area.mode == AllowedAreaMode.Humanlike ? humanIndex : animalIndex).GetValueSafe(area.ID);
		}
		public override void ExposeData()
		{
			Scribe_Collections.Look(ref humanIndex, "humanIndex");
			if (humanIndex == null || humanIndex.Count == 0)
				InitIndexH();

			Scribe_Collections.Look(ref animalIndex, "areaIndex");
			if (animalIndex == null || animalIndex.Count == 0)
				InitIndexA();
		}
	}
}