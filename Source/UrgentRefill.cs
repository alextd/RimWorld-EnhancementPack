using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;
using Harmony;
using AllowTool;

namespace TD_Enhancement_Pack
{
	/* 
	 * Using AllowToolDefOf.HaulingUrgent so can't xml this
	 * 
	<WorkGiverDef>
		<defName>HaulUrgentlyRefill</defName>
		<label>refill zones urgently</label>
		<giverClass>WorkGiver_HaulUrgentlyRefill</giverClass>
		<workType>AllowToolDefOf.HaulingUrgent</workType>
		<verb>refilling urgently</verb>
		<gerund>refilling urgently</gerund>
		<priorityInType>100</priorityInType>
		<directOrderable>false</directOrderable>
		<requiredCapacities>
			<li>Manipulation</li>
		</requiredCapacities>
	</WorkGiverDef>
	*/

	[StaticConstructorOnStartup]
	public static class UrgentRefill
	{
		public static bool active;
		static UrgentRefill()
		{
			active = false;
			if (!Settings.Get().zoneRefill) return;
			try
			{
				TryUrgentRefill();
				active = true;
			}
			catch (Exception) { }
		}
		public static void TryUrgentRefill()
		{ 
			WorkGiverDef urgentRefillDef = new WorkGiverDef()
			{
				defName = "HaulUrgentlyRefill",
				label = "TD.WorkGiverRefillZones".Translate(),
				giverClass = typeof(WorkGiver_HaulUrgentlyRefill),
				workType = AllowToolDefOf.HaulingUrgent,
				verb = "TD.RefillingUrgently".Translate(),
				gerund  = "TD.RefillingUrgently".Translate(),
				priorityInType = 100,
				directOrderable = false,
				requiredCapacities = new List<PawnCapacityDef>() { PawnCapacityDefOf.Manipulation }
			};
			DefDatabase<WorkGiverDef>.Add(urgentRefillDef);
			AllowToolDefOf.HaulingUrgent.workGiversByPriority.Add(urgentRefillDef);
			AllowToolDefOf.HaulingUrgent.workGiversByPriority.SortBy(g => g.priorityInType);
		}
	}

	// Generates hauling jobs for zones designated need urgent refilling
	public class WorkGiver_HaulUrgentlyRefill : WorkGiver_Scanner
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return HaulAIUtility.HaulToStorageJob(pawn, t);
		}

		private static bool NeedsRefill(IntVec3 c, Map map)
		{
			foreach (var thing in map.thingGrid.ThingsListAt(c))
			{
				if (thing.def.EverStorable(false))
				{
					return false;
				}
				if (thing.def.entityDefToBuild != null && thing.def.entityDefToBuild.passability != Traversability.Standable)
				{
					return false;
				}
				if (thing.def.surfaceType == SurfaceType.None && thing.def.passability != Traversability.Standable)
				{
					return false;
				}
			}
			return true;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<SlotGroup> needRefill = pawn.Map.haulDestinationManager.AllGroupsListForReading
				.FindAll(group => group.IsMarkedForRefill(pawn.Map)
					&& group.CellsList.Any(c => NeedsRefill(c, pawn.Map)));

			return pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
				.FindAll(t => !t.IsInValidBestStorage()
				&& HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, false)
				&& needRefill.Any(g => g.Settings.AllowedToAccept(t)));
		}
	}

	//Gizmo button to toggle it on
	[StaticConstructorOnStartup]
	internal static class SlotGroup_GetGizmos_Patch
	{
		public static Texture2D haulUrgentlyIcon = ContentFinder<Texture2D>.Get("haulUrgently", false);

		public static void InsertUrgentRefillGizmos(ref IEnumerable<Gizmo> __result, Map map, ISlotGroupParent parent)
		{
			if (!UrgentRefill.active) return;
			SlotGroup group = parent.GetSlotGroup();
			__result = __result.Add(new Command_Toggle()
			{
				defaultLabel = "TD.GizmoUrgentRefill".Translate(),
				defaultDesc = "TD.GizmoUrgentRefillDesc".Translate(),
				icon = haulUrgentlyIcon,
				isActive = () => group.IsMarkedForRefill(map),
				toggleAction = delegate
				{
					group.MarkForRefill(map, !group.IsMarkedForRefill(map));
				}
			});
		}
	}

	[HarmonyPatch(typeof(Building_Storage), "GetGizmos")]
	internal static class BuildingStorage_GetGizmos_Patch
	{
		[HarmonyPostfix]
		public static void InsertUrgentRefillGizmos(ref IEnumerable<Gizmo> __result, Building_Storage __instance)
		{
			SlotGroup_GetGizmos_Patch.InsertUrgentRefillGizmos(ref __result, __instance.Map, __instance);
		}
	}

	[StaticConstructorOnStartup]
	[HarmonyPatch(typeof(Zone_Stockpile), "GetGizmos")]
	internal static class ZoneStockpile_GetGizmos_Patch
	{
		[HarmonyPostfix]
		public static void InsertUrgentRefillGizmos(ref IEnumerable<Gizmo> __result, Zone_Stockpile __instance)
		{
			SlotGroup_GetGizmos_Patch.InsertUrgentRefillGizmos(ref __result, __instance.Map, __instance);
		}
	}

	//Settings
	public class RefillZoneComp : MapComponent
	{
		public HashSet<Building_Storage> markedForRefillBuilding = new HashSet<Building_Storage>();
		public HashSet<Zone_Stockpile> markedForRefillStockpile = new HashSet<Zone_Stockpile>();

		public RefillZoneComp(Map m) : base(m)
		{

		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref markedForRefillBuilding, "markedForRefillBuilding", LookMode.Reference);
			if (markedForRefillBuilding == null)
				markedForRefillBuilding = new HashSet<Building_Storage>();
			else
				markedForRefillBuilding.RemoveWhere(b => b == null);

			Scribe_Collections.Look(ref markedForRefillStockpile, "markedForRefillStockpile", LookMode.Reference);
			if (markedForRefillStockpile == null)
				markedForRefillStockpile = new HashSet<Zone_Stockpile>();
			else
				markedForRefillStockpile.RemoveWhere(s => s == null);
		}
	}
	
	public static class RefillZoneExtensions
	{ 
		public static bool IsMarkedForRefill(this SlotGroup group, Map map)
		{
			RefillZoneComp comp = map.GetComponent<RefillZoneComp>();
			return comp.markedForRefillBuilding.Contains(group.parent as Building_Storage)
				|| comp.markedForRefillStockpile.Contains(group.parent as Zone_Stockpile);
		}

		public static void MarkForRefill(this SlotGroup group, Map map, bool enable)
		{
			RefillZoneComp comp = map.GetComponent<RefillZoneComp>();
			if (group.parent is Building_Storage building)
			{
				if (enable)
					comp.markedForRefillBuilding.Add(building);
				else
					comp.markedForRefillBuilding.Remove(building);
			}
			else if (group.parent is Zone_Stockpile zone)
			{
				if (enable)
					comp.markedForRefillStockpile.Add(zone);
				else
					comp.markedForRefillStockpile.Remove(zone);
			}
		}
	}

	//And delete from Comp when they are removed from map
	[HarmonyPatch(typeof(HaulDestinationManager), "RemoveHaulDestination")]
	class UrgentRefill_Deletion_Patches
	{
		//public void RemoveHaulDestination(IHaulDestination haulDestination)
		public static void Postfix(Map ___map, IHaulDestination haulDestination)
		{
			if (haulDestination is ISlotGroupParent slotGroupParent &&
				slotGroupParent.GetSlotGroup() is SlotGroup slotGroup)
				slotGroup.MarkForRefill(___map,  false);
		}
	}
}
