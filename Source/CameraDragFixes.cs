using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(CameraMapConfig_Normal), MethodType.Constructor)]
	class CameraDragFixes
	{
		//CameraMapConfig_Normal
		public static void Postfix(CameraMapConfig_Normal __instance)
		{
			Log.Message($"dollyRateMouseDrag is now 2");
			__instance.dollyRateMouseDrag = 2.0f;//harded div by 2 somewhere in camera driver
		}
	}
	[HarmonyPatch(typeof(CameraDriver), "Update")]
	class CameraDriverUpdate
	{
		//CameraMapConfig_Normal
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach(CodeInstruction i in instructions)
			{
				if (i.opcode == OpCodes.Ldc_R4)
				{
					if (i.operand.ChangeType<float>() == 0.7f)//scale factor for zoomed out, from 0 to ?
						i.operand = 1.3f;//trial and errored this
					//else if (i.operand.ChangeType<float>() == 0.3f)//base scroll factor when zoomed in 100%
					//	i.operand = 1.0f;
				}
				yield return i;
			}
		}
	}
}
