using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TD_Enhancement_Pack.Alerts
{
	public class Alert_Heatstroke: Alert
	{
		private IEnumerable<Pawn> BurningPawns
		{
			get
			{
				foreach (Map map in Find.Maps)
				{
					foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
					{
						if (pawn.AmbientTemperature > pawn.SafeTemperatureRange().max && 
							pawn.health.hediffSet.HasHediff(HediffDefOf.Heatstroke, true))
						{
							yield return pawn;
						}
					}
				}
			}
		}

		public Alert_Heatstroke()
		{
			defaultLabel = HediffDefOf.Heatstroke.LabelCap;
			defaultExplanation = "Someone is gaining heatstroke, that's not a good thing";
		}

		public override AlertReport GetReport()
		{
			return Settings.Get().alertHeatstroke ?
				AlertReport.CulpritsAre(BurningPawns) :
				AlertReport.Inactive;
		}
	}
}
