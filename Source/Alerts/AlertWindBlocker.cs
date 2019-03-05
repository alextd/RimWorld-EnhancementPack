using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace TD_Enhancement_Pack.Alerts
{
	public class Alert_WindBlocker : Alert
	{
		public static FieldInfo cellsInfo = AccessTools.Field(typeof(CompPowerPlantWind), "windPathBlockedCells");
		public static List<IntVec3> WindPathBlockedCells(CompPowerPlantWind comp) =>
			(List<IntVec3>)cellsInfo.GetValue(comp);
		private IEnumerable<GlobalTargetInfo> BlockerCells
		{
			get
			{
				foreach (Map map in Find.Maps)
					foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
						if (thing.TryGetComp<CompPowerPlantWind>() is CompPowerPlantWind comp)
							foreach (IntVec3 cell in WindPathBlockedCells(comp))
								yield return new GlobalTargetInfo(cell, map);
			}
		}

		public Alert_WindBlocker()
		{
			defaultLabel = "TD.WindBlocked".Translate();
			defaultExplanation = "TD.WindBlockedDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return Settings.Get().alertWindBlocker ?
				AlertReport.CulpritsAre(BlockerCells) :
				AlertReport.Inactive;
		}
	}
}