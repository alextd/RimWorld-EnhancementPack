using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	static class CameraFixPatches
	{
		static CameraFixPatches()
		{
			if (!Settings.Get().cameraDragFixes) return;
			Harmony harmony = new Harmony("Uuugggg.rimworld.TD_Enhancement_Pack.main");

			harmony.Patch(AccessTools.Constructor(typeof(CameraMapConfig_Normal)),
				postfix: new HarmonyMethod(typeof(CameraDragFixes), "Postfix"));
			harmony.Patch(AccessTools.Method(typeof(CameraDriver), "Update"),
				postfix: new HarmonyMethod(typeof(FixUpdate), "Postfix"), transpiler: new HarmonyMethod(typeof(FixUpdate), "Transpiler"));
			harmony.Patch(AccessTools.Method(typeof(CameraDriver), "OnGUI"),
				prefix: new HarmonyMethod(typeof(FixOnGUI), "Prefix"), transpiler: new HarmonyMethod(typeof(FixOnGUI), "Transpiler"));
		}
	}


	//[HarmonyPatch(typeof(CameraMapConfig_Normal), MethodType.Constructor)]
	class CameraDragFixes
	{
		//CameraMapConfig_Normal
		public static void Postfix(CameraMapConfig_Normal __instance)
		{
			__instance.dollyRateMouseDrag = 2.0f;//harded div by 2 somewhere in camera driver
		}
	}

	//don't set this.mouseDragVect = Vector2.zero at end of OnGUI;
	//set this.mouseDragVect = Vector2.zero in a prefix
	//so Update can see this.mouseDragVect is not zero
	//[HarmonyPatch(typeof(CameraDriver), "OnGUI")]
	static class FixOnGUI
	{
		//private Vector2 mouseDragVect = Vector2.zero;
		public static FieldInfo mouseDragInfo = AccessTools.Field(typeof(CameraDriver), "mouseDragVect");
		public static Vector2 MouseDrag(this CameraDriver driver) => (Vector2)mouseDragInfo.GetValue(driver);
		public static void SetMouseDrag(this CameraDriver driver, Vector2 val) => mouseDragInfo.SetValue(driver, val);
		public static void Prefix(CameraDriver __instance)
		{
			__instance.SetMouseDrag(Vector2.zero);
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo Vector2Zero = AccessTools.Property(typeof(Vector2), "zero").GetGetMethod();

			List<CodeInstruction> instList = instructions.ToList();

			for (int i = 0; i < instList.Count; i++)
			{
				//IL_025a: ldarg.0      // this
				//IL_025b: call valuetype[UnityEngine]UnityEngine.Vector2[UnityEngine]UnityEngine.Vector2::get_zero()
				//IL_0260: stfld valuetype[UnityEngine]UnityEngine.Vector2 Verse.CameraDriver::mouseDragVect
				CodeInstruction inst = instList[i];
				if (inst.IsLdarg(0) &&
					instList[i + 1].Calls(Vector2Zero) &&
					instList[i + 2].StoresField(mouseDragInfo))
				{
					i += 2;//skip next two, all three
				}
				else
					yield return inst;
			}
		}
	}

	[HarmonyPatch(typeof(CameraDriver), "Update")]
	static class FixUpdate
	{
		public static FieldInfo velocity = AccessTools.Field(typeof(CameraDriver), "velocity");
		public static void Postfix(CameraDriver __instance)
		{
			if (__instance.MouseDrag() != Vector2.zero)
			{
				velocity.SetValue(__instance, Vector3.zero);
			}
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//HitchReduceFactor
			MethodInfo HitchReduceFactor = AccessTools.Property(typeof(CameraDriver), "HitchReduceFactor").GetGetMethod();

			foreach (CodeInstruction i in instructions)
			{
				if (i.LoadsConstant(0.7f))
				{
					//scale factor for zoomed out, from 0 to ?
					i.operand = 1.3f;//trial and errored this
					//else if (i.operand.ChangeType<float>() == 0.3f)//base scroll factor when zoomed in 100%
					//	i.operand = 1.0f;
				}
				yield return i;
				
				if (i.Calls(HitchReduceFactor))
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FixUpdate), nameof(ChangeHitch)));
				}
			}
		}

		public static float ChangeHitch(float result)
		{
			CameraDriver driver = Find.CameraDriver;
			if (driver.MouseDrag() != Vector2.zero)
			{
				return 1 / RealTime.deltaTime / 60f;
			}
			return result;
		}
	}
}
