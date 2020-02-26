using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TD_Enhancement_Pack.Alerts
{
	public class Alert_Burning : Alert_Critical
	{
		private IEnumerable<Pawn> BurningPawns
		{
			get
			{
				foreach (Map map in Find.Maps)
				{
					foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
					{
						if (pawn.AmbientTemperature > pawn.SafeTemperatureRange().max + 150f)//150f hardcoded in HediffGiver_Heat
						{
							yield return pawn;
						}
					}
				}
			}
		}

		public Alert_Burning()
		{
			defaultLabel = "Burning".Translate();
			defaultExplanation = "TD.AlertBurning".Translate();
		}

		public override AlertReport GetReport()
		{
			return Settings.Get().alertBurning ?
				AlertReport.CulpritsAre(BurningPawns.ToList()) :
				AlertReport.Inactive;
		}
	}
}
