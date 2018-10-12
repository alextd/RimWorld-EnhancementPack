using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
	class SmoothSelectedStone
	{
		//private void InitDesignators()
		public static void Postfix(List<Designator> ___desList)
		{
			Designator des = new Designator_SmoothSurface();
			if (Current.Game.Rules.DesignatorAllowed(des))
				___desList.Add(des);
		}
	}

	[HarmonyPatch(typeof(Designator), "CanDesignateThing")]
	public static class SmoothCanDesignateThing
	{
		//public virtual AcceptanceReport CanDesignateThing(Thing t)
		public static bool Prefix(ref AcceptanceReport __result, Designator __instance, Thing t)
		{
			if (__instance is Designator_SmoothSurface des)
			{
				if (EdificeUtility.IsEdifice(t.def) && t.def.IsSmoothable &&
					__instance.Map.designationManager.DesignationAt(t.Position, DesignationDefOf.SmoothWall) == null)
					__result = AcceptanceReport.WasAccepted;

				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(Designator), "DesignateThing")]
	public static class SmoothDesignateThing
	{
		//public virtual void DesignateThing(Thing t)
		public static bool Prefix(Designator __instance, Thing t)
		{
			if (__instance is Designator_SmoothSurface des)
			{
				IntVec3 pos = t.Position;
				if (DebugSettings.godMode)
				{
					SmoothableWallUtility.SmoothWall(t, __instance.Map.mapPawns.FreeColonistsSpawned.First());
				}
				else
				{
					des.Map.designationManager.AddDesignation(new Designation(pos, DesignationDefOf.SmoothWall));
				}
				des.Map.designationManager.TryRemoveDesignation(pos, DesignationDefOf.Mine);

				return false;
			}
			return true;
		}
	}
}
