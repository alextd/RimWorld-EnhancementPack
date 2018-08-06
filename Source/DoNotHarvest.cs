using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Zone_Growing), "GetGizmos")]
	class DoNotHarvest
	{
		//public override IEnumerable<Gizmo> GetGizmos()
		static void Postfix(Zone_Growing __instance, ref IEnumerable<Gizmo> __result)
		{
			if (!Settings.Get().zoneHarvestableToggle) return;

			List<Gizmo> result = new List<Gizmo>(__result);

			Gizmo harvestGizmo = new Command_Toggle
			{
				defaultLabel = "TD.AllowHarveseting".Translate(),
				defaultDesc = "TD.AllowHarvesetingDesc".Translate(),
				icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/Designators/Harvest", true),
				isActive = (() => __instance.CanHarvest()),
				toggleAction = delegate
				{
					__instance.ToggleHarvest();
				}
			};
			result.Add(harvestGizmo);

			// make caller use the list
			__result = result.AsEnumerable();
		}
	}
	public class MapComponent_ZoneForbidHarvest : MapComponent
	{
		public MapComponent_ZoneForbidHarvest(Map map) : base(map)
		{ harvestForbidden = new List<Zone_Growing>(); }
		public List<Zone_Growing> harvestForbidden;

		public override void ExposeData()
		{
			//Scribe_Collections.Look(ref harvestForbidden, "harvestForbidden", LookMode.Reference); //1.0 lets this be ILoadReferenceable
		}

	}

	public static class Zone_Growing_Extensions
	{
		public static bool CanHarvest(this Zone_Growing zone)
		{
			return !zone.Map.GetComponent<MapComponent_ZoneForbidHarvest>().harvestForbidden.Contains(zone);
		}
		public static void ToggleHarvest(this Zone_Growing zone)
		{
			if (zone.Map.GetComponent<MapComponent_ZoneForbidHarvest>().harvestForbidden.Contains(zone))
				zone.Map.GetComponent<MapComponent_ZoneForbidHarvest>().harvestForbidden.Remove(zone);
			else
				zone.Map.GetComponent<MapComponent_ZoneForbidHarvest>().harvestForbidden.Add(zone);

		}
	}
	
	[HarmonyPatch(typeof(WorkGiver_GrowerHarvest), "HasJobOnCell")] //Tried ExtraRequirements and it was inconsistent?
	public static class NoHarvestJob
	{
		//public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
		public static bool Prefix(Pawn pawn, IntVec3 c, ref bool __result)
		{
			if(pawn.Map.zoneManager.ZoneAt(c) is Zone_Growing zone 
				&& !zone.CanHarvest())
			{ 
				__result = false;
				return false;
			}
			return true;
		}
	}
}