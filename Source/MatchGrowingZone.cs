using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;

namespace TD_Enhancement_Pack
{
	class Designator_AddMatch_Growing: Designator_ZoneAdd_Growing
	{
		public Designator_AddMatch_Growing() : base()
		{
			this.defaultLabel = "Match";
			this.defaultDesc = "Add a growing zone, but only add terrain with fertility that matches the first cell picked";
		}
		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!base.CanDesignateCell(c).Accepted)
			{
				return false;
			}

			FertilityGrid grid = Map.fertilityGrid;
			if (grid.FertilityAt(c) < ThingDefOf.Plant_Potato.plant.fertilityMin)
			{
				return false;
			}
			if (Find.DesignatorManager.Dragger.Dragging)
			{
				IntVec3 startDragCell = (IntVec3)AccessTools.Field(typeof(DesignationDragger), "startDragCell").GetValue(Find.DesignatorManager.Dragger);
				if (grid.FertilityAt(c) != grid.FertilityAt(startDragCell))
					return false;
			}
			
			return true;
		}
	}

	[HarmonyPatch(typeof(Zone_Growing), "GetZoneAddGizmos")]
	public static class SelectedZoneMatchGizmo
	{
		//public override IEnumerable<Gizmo> GetZoneAddGizmos()
		public static void Postfix(ref IEnumerable<Gizmo> __result)
		{
			List<Gizmo> result = __result.ToList();
			result.Add(DesignatorUtility.FindAllowedDesignator<Designator_AddMatch_Growing>());
			__result = result;
		}
	}
}
