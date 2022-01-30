using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	class Designator_AddMatch_Growing: Designator_ZoneAdd_Growing
	{
		public Designator_AddMatch_Growing() : base()
		{
			this.defaultLabel = "TD.Match".Translate();
			this.defaultDesc = "TD.MatchDesc".Translate();
		}
		
		public static FieldInfo startDragCellInfo = AccessTools.Field(typeof(DesignationDragger), "startDragCell");
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
				IntVec3 startDragCell = (IntVec3)startDragCellInfo.GetValue(Find.DesignatorManager.Dragger);
				if (grid.FertilityAt(c) != grid.FertilityAt(startDragCell))
					return false;
			}
			
			return true;
		}

		public override bool Visible => Settings.settings.matchGrowButton && base.Visible;
	}

	[HarmonyPatch(typeof(Zone_Growing), "GetZoneAddGizmos")]
	public static class SelectedZoneMatchGizmo
	{
		//public override IEnumerable<Gizmo> GetZoneAddGizmos()
		public static void Postfix(ref IEnumerable<Gizmo> __result)
		{
			List<Gizmo> result = new List<Gizmo> ();
			result.Add(DesignatorUtility.FindAllowedDesignator<Designator_AddMatch_Growing>());
			result.AddRange(__result);
			__result = result;
		}
	}
}
