using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(JobDriver_Flee), "MakeNewToils")]
	public static class StopFlee
	{
		//protected override IEnumerable<Toil> MakeNewToils()
		public static void Postfix(ref IEnumerable<Toil> __result, JobDriver_Flee __instance)
		{
			if(!Settings.Get().stopFlee)	return;

			List<Toil> result = __result.ToList();

			Toil goToil = result.Last();
			goToil.AddEndCondition(delegate
			{
				Thing instigator = __instance.job.GetTarget(TargetIndex.B).Thing;
				if (instigator is Pawn pawn)
				{
					if (pawn.Downed || pawn.Destroyed || pawn.Dead)
					{
						Log.Message($"{__instance.GetActor()} instigator {instigator} is down");
						return JobCondition.Succeeded;
					}
				}
				else if (!SelfDefenseUtility.ShouldStartFleeing(__instance.GetActor()))
				{
					Log.Message($"{__instance.GetActor()} no longer scared");
					return JobCondition.Succeeded;
				}
				return JobCondition.Ongoing;
			});

			__result = result;
		}
	}
}
