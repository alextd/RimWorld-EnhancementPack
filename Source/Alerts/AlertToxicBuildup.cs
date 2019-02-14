using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TD_Enhancement_Pack.Alerts
{
	public class Alert_Toxic : Alert
	{
		private IEnumerable<Pawn> BurningPawns
		{
			get
			{
				foreach (Map map in Find.Maps)
				{
					foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
					{
						if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup, true)?.Severity > .2f)
						{
							yield return pawn;
						}
					}
				}
			}
		}

		public Alert_Toxic()
		{
			defaultLabel = HediffDefOf.ToxicBuildup.LabelCap;
			defaultExplanation = "A colonist has non-trivial levels of toxic buildup. That's not a good thing.";
		}

		public override AlertReport GetReport()
		{
			return Settings.Get().alertToxic ?
				AlertReport.CulpritsAre(BurningPawns) :
				AlertReport.Inactive;
		}
	}
}
