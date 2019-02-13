using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using Harmony;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.Update))]
	class ZoomToMouse
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			FieldInfo rootSizeInfo = AccessTools.Field(typeof(CameraDriver), "rootSize");

			foreach (var i in instructions)
			{
				if(i.opcode == OpCodes.Stfld && i.operand == rootSizeInfo)
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ZoomToMouse), nameof(Adjust)));
				}
				else yield return i;
			}
		}
		
		public static FieldInfo rootPosInfo = AccessTools.Field(typeof(CameraDriver), "rootPos");
		public static FieldInfo rootSizeInfo = AccessTools.Field(typeof(CameraDriver), "rootSize");
		public static MethodInfo ApplyInfo = AccessTools.Method(typeof(CameraDriver), "ApplyPositionToGameObject");
		public static void Adjust(CameraDriver driver, float rootSize)
		{
			//Find what old positions are
			Vector3 rootPos = (Vector3)rootPosInfo.GetValue(driver);
			Vector3 oldMousePos = UI.MouseMapPosition();

			//apply new zoom (and ApplyPositionToGameObject so that MouseMapPosition is updated)
			rootSizeInfo.SetValue(driver, rootSize);
			ApplyInfo.Invoke(driver, null);

			//Find new mouse pos
			Vector3 newMousePos = UI.MouseMapPosition();

			//adjust for mouse pos difference: keep mousepos at the same spot.
			rootPos += oldMousePos - newMousePos;

			rootPosInfo.SetValue(driver, rootPos);
		}
	}
}
