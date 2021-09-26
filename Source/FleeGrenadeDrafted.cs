﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	public class ThinkNode_ConditionalCanDoDraftedConstantThinkTreeJobNow : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return Settings.Get().dodgeGrenade &&
				!pawn.Downed && !pawn.IsBurning() && !pawn.InMentalState && pawn.Awake() &&
				(!Settings.Get().dodgeGrenadeUnlessBelt || !pawn.apparel.WornApparel.Any(a => a is ShieldBelt));
		}
	}


	//Flee Enemy Grenades too
	[HarmonyPatch(typeof(Projectile_Explosive), "Impact")]
	public class NotifyEnemiesOfGrenade
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return Transpilers.MethodReplacer(instructions,
				AccessTools.Method(typeof(GenExplosion), nameof(GenExplosion.NotifyNearbyPawnsOfDangerousExplosive)),
				AccessTools.Method(typeof(NotifyEnemiesOfGrenade), nameof(NOFACTIONNotifyNearbyPawnsOfDangerousExplosive)));
		}

		//public static void NotifyNearbyPawnsOfDangerousExplosive(Thing exploder, DamageDef damage, Faction onlyFaction = null)
		public static void NOFACTIONNotifyNearbyPawnsOfDangerousExplosive(Thing exploder, DamageDef damage, Faction onlyFaction = null, Thing launcher = null)
		{
			Faction actualFaction = onlyFaction;
			if (Settings.Get().dodgeGrenadeEnemy && onlyFaction != Faction.OfPlayer)
				actualFaction = null;
			if (Settings.Get().dodgeGrenadeNPC && onlyFaction == Faction.OfPlayer)
				actualFaction = null;
			GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(exploder, damage, actualFaction, launcher);
		}
	}


	[HarmonyPatch(typeof(Pawn_MindState), "Notify_DangerousExploderAboutToExplode")]
	public static class Pawn_MindState_Drafted
	{
		//internal void Notify_DangerousExploderAboutToExplode(Thing exploder)
		public static void Postfix(Pawn_MindState __instance, Thing exploder)
		{
			if (!Settings.Get().dodgeGrenade) return; 

			//Just copy-paste vanilla but do it for drafted now.
			if ((int)__instance.pawn.RaceProps.intelligence >= 2 && __instance.pawn.Drafted)
			{
				__instance.knownExploder = exploder;
				__instance.pawn.jobs.CheckForJobOverride();
			}
		}
	}
}
