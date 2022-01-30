using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;


namespace TD_Enhancement_Pack
{
	public class Alert_NoBill : Alert
	{
		private IEnumerable<Thing> IdleBenches
		{
			get
			{
				foreach (Map map in Find.Maps)
				{
					foreach (Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
					{
						if (!thing.IsForbidden(Faction.OfPlayer) &&
							thing is Building_WorkTable workTable &&
							workTable.GetInspectTabs()?.Count() > 0 &&
							workTable.BillStack.Count == 0)
						{
							yield return thing;
						}
					}
				}
			}
		}

		public Alert_NoBill()
		{
			defaultLabel = "TD.EmptyWorkbench".Translate();
			defaultExplanation = "TD.EmptyWorkbenchDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return Settings.settings.alertNoBill ?
				AlertReport.CulpritsAre(IdleBenches.ToList()) :
				AlertReport.Inactive;
		}
	}
}
