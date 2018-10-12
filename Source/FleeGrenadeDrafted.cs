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
	public class ThinkNode_ConditionalCanDoDraftedConstantThinkTreeJobNow : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return Settings.Get().dodgeGrenade &&
				!pawn.Downed && !pawn.IsBurning() && !pawn.InMentalState && pawn.Awake();
		}
	}
}
