using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(IncidentWorker_ResourcePodCrash), "TryExecuteWorker")]
	class ResourcePodCrashContents
	{
		//protected override bool TryExecuteWorker(IncidentParms parms)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo ReceiveLetterInfo = AccessTools.Method(typeof(LetterStack), "ReceiveLetter", new Type[]
					{ typeof(string), typeof(string), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(string)});
			
			MethodInfo GenerateInfo = AccessTools.Method(typeof(ThingSetMaker), "Generate");
			
			foreach(CodeInstruction i in instructions)
			{
				if (i.Calls(ReceiveLetterInfo))
					i.operand = AccessTools.Method(typeof(ResourcePodCrashContents), nameof(ReceiveLetterAppend));

				yield return i;

				if (i.Calls(GenerateInfo))
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
			if(Settings.Get().dropPodWhatDropped)
				text += "\n\n" + "TD.WhatDropped".Translate(thingLabel);
			stack.ReceiveLetter(label, text, textLetterDef, lookTargets, relatedFaction, debugInfo);
		}
	}
}
