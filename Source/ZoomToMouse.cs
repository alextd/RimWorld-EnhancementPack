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
			if(!Settings.Get().zoomToMouse || Event.current.shift)
			{
				rootSizeInfo.SetValue(driver, rootSize);
				return;
			}
			//Find what old position is
			Vector3 rootPos = (Vector3)rootPosInfo.GetValue(driver);

			//update the Camera Object (for previously-done scrolling movement) and get old mouse pos
			ApplyInfo.Invoke(driver, null);
			Vector3 oldMousePos = UI.MouseMapPosition();

			//apply new zoom
			rootSizeInfo.SetValue(driver, rootSize);

			//update the Camera Object for the zoom, and get NEW mouse pos
			ApplyInfo.Invoke(driver, null);
			Vector3 newMousePos = UI.MouseMapPosition();

			//adjust for mouse pos difference: keep mousepos at the same spot.
			rootPos += oldMousePos - newMousePos;

			rootPosInfo.SetValue(driver, rootPos);
		}
	}
}
