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
					{ typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(LookTargets), typeof(Faction), typeof(Quest), typeof(List<ThingDef>), typeof(string)});
			
			MethodInfo GenerateInfo = AccessTools.Method(typeof(ThingSetMaker), "Generate");
			
			foreach(CodeInstruction i in instructions)
			{
				if (i.Calls(ReceiveLetterInfo))
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResourcePodCrashContents), nameof(ReceiveLetterAppend)));

				else yield return i;

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

		//public void ReceiveLetter(TaggedString label, TaggedString text, LetterDef textLetterDef, LookTargets lookTargets, Faction relatedFaction = null, Quest quest = null, List<ThingDef> hyperlinkThingDefs = null, string debugInfo = null)
		public static void ReceiveLetterAppend(LetterStack stack, TaggedString label, TaggedString text, LetterDef textLetterDef, LookTargets lookTargets, Faction relatedFaction = null, Quest quest = null, List<ThingDef> hyperlinkThingDefs = null, string debugInfo = null)
		{
			if(Settings.Get().dropPodWhatDropped)
				text += "\n\n" + "TD.WhatDropped".Translate(thingLabel);
			stack.ReceiveLetter(label, text, textLetterDef, lookTargets, relatedFaction, quest, hyperlinkThingDefs, debugInfo);
		}
	}
}
