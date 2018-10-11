using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;


namespace TD_Enhancement_Pack
{
	public class Alert_Deterioration : Alert
	{
		private IEnumerable<Thing> DeterioratingThings
		{
			get
			{
				foreach(Map map in Find.Maps)
				{
					foreach(Thing thing in map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
					{
						if (!thing.IsForbidden(Faction.OfPlayer) &&
							SteadyEnvironmentEffects.FinalDeteriorationRate(thing) > 0 &&
							thing.HitPoints < thing.MaxHitPoints * 0.5f &&
							!thing.Position.Fogged(map))
						{
							yield return thing;
						}
					}
				}
			}
		}

		public Alert_Deterioration()
		{
			defaultLabel = "Deterioration";
			defaultExplanation = "Something is deteriorating and is getting low on health. Put it indoors to keep it safe.";
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(DeterioratingThings);
		}
	}
}
