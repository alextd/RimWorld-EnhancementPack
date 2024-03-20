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
			//protected void SendStandardLetter(TaggedString baseLetterLabel, TaggedString baseLetterText, LetterDef baseLetterDef, IncidentParms parms, LookTargets lookTargets, params NamedArgument[] textArgs)
			MethodInfo SendStandardLetterInfo = AccessTools.Method(typeof(IncidentWorker), "SendStandardLetter",
				[typeof(TaggedString), typeof(TaggedString), typeof(LetterDef), typeof(IncidentParms), typeof(LookTargets), typeof(NamedArgument[])]);
			
			MethodInfo GenerateInfo = AccessTools.Method(typeof(ThingSetMaker), "Generate");


			foreach (CodeInstruction i in instructions)
			{
				if (i.Calls(SendStandardLetterInfo))
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResourcePodCrashContents), nameof(SendStandardLetterAppend)));
				else
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

		//protected void SendStandardLetter(TaggedString baseLetterLabel, TaggedString baseLetterText, LetterDef baseLetterDef, IncidentParms parms, LookTargets lookTargets, params NamedArgument[] textArgs)
		public static void SendStandardLetterAppend(IncidentWorker_ResourcePodCrash instance, TaggedString baseLetterLabel, TaggedString baseLetterText, LetterDef baseLetterDef, IncidentParms parms, LookTargets lookTargets, NamedArgument[] textArgs)
		{
			if(Mod.settings.dropPodWhatDropped)
				baseLetterText += "\n\n" + "TD.WhatDropped".Translate(thingLabel);
			//			instance.SendStandardLetter ; //private :(
			IncidentWorker.SendIncidentLetter(baseLetterLabel, baseLetterText, baseLetterDef, parms, lookTargets, instance.def, textArgs);
		}
	}
}
