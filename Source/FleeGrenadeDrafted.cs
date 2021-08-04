using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;


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
	public static class Pawn_MindState_Notify_DangerousExploderAboutToExplode_Patch
	{
		/// <summary>
		/// Harmony Transpiler to change that drafted pawns also move away from explosions
		/// </summary>
		/// <explanation>
		/// The transpiler adds an additional check (CheckDodgeGrenade), which is called from 
		/// Assembly-CSharp.dll --> Verse.AI.Pawn_MindState.Notify_DangerousExploderAboutToExplode 
		/// This check is nessecery to the additional condition "&& !this.pawn.Drafted" in Notify_DangerousExploderAbouttoExplode in RimWorld_1.3
		/// 
		/// The Transpiler search for the Command "Boolean get_Drafted()" in the IL-Code in Notity_DangerousExploderAboutToExplode
		/// and adds after this command the call to the CheckDodgeGrenade instead for only checking the draftet status of the pawn.
		/// </explanation>
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			bool found = false;
			foreach (var instruction in instructions)
			{
				yield return instruction;
				if ((instruction.operand ?? string.Empty).ToString() == "Boolean get_Drafted()")
				{
					yield return new CodeInstruction(OpCodes.Call, m_CheckDodgeGrenade);
					found = true;
				}
			}
			if (found is false)
				Verse.Log.Error("Cannot find 'Boolean get_Drafted()' in Pawn_MindState_Notify_DangerousExploderAboutToExplode");
		}
		/// <summary>
		/// If the setting dodgeGrenade is turned on (TRUE) then the function always return FALSE.
		/// Else the function returns the original drafted value
		/// </summary>
		/// <param name="drafted"></param>
		/// <returns>FALSE for notify pawn about dangerous explosion</returns>
		public static bool CheckDodgeGrenade(bool drafted)
		{
			return !Settings.Get().dodgeGrenade && drafted;
		}
		static MethodInfo m_CheckDodgeGrenade = SymbolExtensions.GetMethodInfo(() => CheckDodgeGrenade(true));
	}
}
