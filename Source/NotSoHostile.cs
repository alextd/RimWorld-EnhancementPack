using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetAttackNearbyEnemyJob")]
	public static class NotSoHostile
	{
		//JobGiver_ConfigurableHostilityResponse
		//private Job TryGetAttackNearbyEnemyJob(Pawn pawn)	
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions )
		{
			//AttackTargetFinder.BestAttackTarget
			MethodInfo BestAttackTargetInfo = AccessTools.Method(typeof(AttackTargetFinder), "BestAttackTarget");

			MethodInfo BetterAttackTargetInfo = AccessTools.Method(typeof(NotSoHostile), "BetterAttackTarget");
			foreach (CodeInstruction i in instructions)
			{
				if (i.Calls(BestAttackTargetInfo))
					yield return new CodeInstruction(OpCodes.Call, BetterAttackTargetInfo);
				else
					yield return i;
			}
		}

		//public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDist = 0, float maxDist = 9999, IntVec3 locus = default(IntVec3), float maxTravelRadiusFromLocus = float.MaxValue, bool canBash = false, bool canTakeTargetsCloserThanEffectiveMinRange = true);
		public static IAttackTarget BetterAttackTarget(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDist = 0f, float maxDist = 9999f, IntVec3 locus = default(IntVec3), float maxTravelRadiusFromLocus = 3.40282347E+38f, bool canBash = false, bool canTakeTargetsCloserThanEffectiveMinRange = true)
		{
			return AttackTargetFinder.BestAttackTarget(searcher, flags,
				Settings.Get().ignoreSleepingEnemies ? ThingIsNotSleeping : validator,  //validator is null for TryGetAttackNearbyEnemyJob, otherwise this is totally broken
				minDist, maxDist, locus, maxTravelRadiusFromLocus, canBash, canTakeTargetsCloserThanEffectiveMinRange);
		}

		public static bool ThingIsNotSleeping(Thing t)
		{
			if(t is Pawn pawn)
			{
				return pawn.Awake();
			}
			return true;
		}
	}
}
