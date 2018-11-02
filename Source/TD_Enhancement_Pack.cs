using System.Reflection;
using System;
using Verse;
using UnityEngine;
using Harmony;
using RimWorld;

namespace TD_Enhancement_Pack
{
	public class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{
			TD.Utilities.HugsLibUpdateNews.MakeNews(this);
			// initialize settings
			GetSettings<Settings>();
#if DEBUG
			HarmonyInstance.DEBUG = true;
#endif
			HarmonyInstance harmony = HarmonyInstance.Create("Uuugggg.rimworld.TD_Enhancement_Pack.main");
			
			//Turn off DefOf warning since harmony patches trigger it.
			harmony.Patch(AccessTools.Method(typeof(DefOfHelper), "EnsureInitializedInCtor"),
				new HarmonyMethod(typeof(Mod), "EnsureInitializedInCtorPrefix"), null);
			
			harmony.PatchAll();
		}

		public static bool EnsureInitializedInCtorPrefix()
		{
			//No need to display this warning.
			return false;
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			GetSettings<Settings>().DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "TDEnhancementPack".Translate();
		}
	}
}