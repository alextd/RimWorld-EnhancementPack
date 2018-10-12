using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(IncidentWorker_ResourcePodCrash), "TryExecuteWorker")]
	class ResourcePodCrashContents
	{
		//protected override bool TryExecuteWorker(IncidentParms parms)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach(CodeInstruction i in instructions)
			{
				if (i.opcode == OpCodes.Callvirt && i.operand == AccessTools.Method(typeof(LetterStack), "ReceiveLetter", new Type[]
					{ typeof(string), typeof(string), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(string)}))
					i.operand = AccessTools.Method(typeof(ResourcePodCrashContents), nameof(ReceiveLetterAppend));

				yield return i;

				if (i.opcode == OpCodes.Callvirt && i.operand == AccessTools.Method(typeof(ThingSetMaker), "Generate"))
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResourcePodCrashContents), nameof(GetThingLabel)));
			}
		}

		public static string thingLabel;
		public static List<Thing> GetThingLabel(List<Thing> things)
		{
			thingLabel = things[0].LabelNoCount;
			return things;
		}

		public static void ReceiveLetterAppend(LetterStack stack, string label, string text, LetterDef textLetterDef, LookTargets lookTargets, Faction relatedFaction = null, string debugInfo = null)
		{
			text += "\n\n" + "TD.WhatDropped".Translate(thingLabel);
			stack.ReceiveLetter(label, text, textLetterDef, lookTargets, relatedFaction, debugInfo);
		}
	}
}
