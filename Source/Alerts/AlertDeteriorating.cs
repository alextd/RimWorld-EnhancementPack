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
							!thing.Position.Fogged(map) &&
							(!(thing is Corpse corpse) || corpse.GetRotStage() == RotStage.Fresh))
						{
							yield return thing;
						}
					}
				}
			}
		}

		public Alert_Deterioration()
		{
			defaultLabel = "TD.Deterioration".Translate();
			defaultExplanation = "TD.DeteriorationAlert".Translate();
		}

		public override AlertReport GetReport()
		{
			return Settings.Get().alertDeteriorating ? 
				AlertReport.CulpritsAre(DeterioratingThings.ToList()) :
				AlertReport.Inactive;
		}
	}
}
