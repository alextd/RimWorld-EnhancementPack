using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Verse;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(CameraDriver), "Update")]
	public static class CameraPanStop
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo HitchReduceFactorInfo = AccessTools.Property(typeof(CameraDriver), "HitchReduceFactor").GetGetMethod();

			foreach (CodeInstruction i in instructions)
			{
				yield return i;
				if(i.opcode == OpCodes.Call && i.operand == HitchReduceFactorInfo)
				{
					yield return new CodeInstruction(OpCodes.Pop);//nevermind
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);//1
				}
			}
		}

		public static void Postfix(CameraDriver __instance)
		{
			FieldInfo velocityInfo = AccessTools.Field(typeof(CameraDriver), "velocity");
			Vector3 velocity = (Vector3)velocityInfo.GetValue(__instance);

			if (velocity != Vector3.zero)
			{
				float skippedFrames = (Time.deltaTime - Time.fixedDeltaTime) / Time.fixedDeltaTime;

				float decay = __instance.config.camSpeedDecayFactor;
				velocity *= (float)Math.Pow(decay, skippedFrames);

				if (velocity.magnitude < 0.1f)
					velocity = Vector3.zero;

				velocityInfo.SetValue(__instance, velocity);
			}
		}
	}
}
