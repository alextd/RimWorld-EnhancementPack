using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using static System.Reflection.Emit.OpCodes;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace TD_Enhancement_Pack
{
	// Multiple people also clear things out of the way for construction as well.

	[HarmonyPatch(typeof(GenConstruct), "HandleBlockingThingJob")]
	class HandleAllBlockingThings
	{
		//public static Job HandleBlockingThingJob(Thing constructible, Pawn worker, bool forced = false)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo FirstBlockingThingInfo = AccessTools.Method(typeof(GenConstruct), "FirstBlockingThing");

			MethodInfo FirstReservableBlockingThingInfo = AccessTools.Method(typeof(HandleAllBlockingThings), nameof(FirstReservableBlockingThing));

			foreach (CodeInstruction i in instructions)
			{
				if (i.Calls(FirstBlockingThingInfo))
					yield return new CodeInstruction(OpCodes.Call, FirstReservableBlockingThingInfo);
				else
					yield return i;
			}
		}

		public static Thing FirstReservableBlockingThing(Thing constructible, Pawn pawnToIgnore)
		{
			if (!Mod.settings.handleAllBlockingThings)
				return GenConstruct.FirstBlockingThing(constructible, pawnToIgnore);

			Thing thing = constructible is Blueprint b ? GenConstruct.MiniToInstallOrBuildingToReinstall(b) : null;

			foreach(var pos in constructible.OccupiedRect())
				foreach(Thing t in pos.GetThingList(constructible.Map))
					if (GenConstruct.BlocksConstruction(constructible, t) && t != pawnToIgnore && t != thing && pawnToIgnore.CanReserve(t))
						return t;

			return null;
		}
	}
}
