using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;
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
				if(i.StoresField(rootSizeInfo))
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ZoomToMouse), nameof(Adjust)));
				}
				else yield return i;
			}
		}

		public static AccessTools.FieldRef<CameraDriver, Vector3> RootPos =
			AccessTools.FieldRefAccess<CameraDriver, Vector3>("rootPos");
		public static AccessTools.FieldRef<CameraDriver, float> RootSize =
			AccessTools.FieldRefAccess<CameraDriver, float>("rootSize");
		public delegate void ApplyPositionToGameObjectDel(CameraDriver cam);
		public static ApplyPositionToGameObjectDel ApplyPositionToGameObject =
			AccessTools.MethodDelegate<ApplyPositionToGameObjectDel>(AccessTools.Method(typeof(CameraDriver), "ApplyPositionToGameObject"));

		public static void Adjust(CameraDriver driver, float rootSize)
		{
			if(!Mod.settings.zoomToMouse || Event.current.shift)
			{
				RootSize(driver) = rootSize;
				return;
			}
			//Find what old position is
			Vector3 rootPos = RootPos(driver);

			//update the Camera Object (for previously-done scrolling movement) and get old mouse pos
			ApplyPositionToGameObject(driver);
			Vector3 oldMousePos = UI.MouseMapPosition();

			//apply new zoom
			RootSize(driver) = rootSize;

			//update the Camera Object for the zoom, and get NEW mouse pos
			ApplyPositionToGameObject(driver);
			Vector3 newMousePos = UI.MouseMapPosition();

			//adjust for mouse pos difference: keep mousepos at the same spot.
			rootPos += oldMousePos - newMousePos;

			RootPos(driver) = rootPos;
		}
	}
}
