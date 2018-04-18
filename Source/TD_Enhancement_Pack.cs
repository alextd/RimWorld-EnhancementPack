using System.Reflection;
using System;
using Verse;
using UnityEngine;
using Harmony;
using RimWorld;

namespace TD_Enhancement_Pack
{
	public class ThisMod : Verse.Mod
	{
		public HarmonyInstance harmony;

		public ThisMod(ModContentPack content) : base(content)
		{
			// initialize settings
			GetSettings<Settings>();
#if DEBUG
			HarmonyInstance.DEBUG = true;
#endif
			harmony = HarmonyInstance.Create("Uuugggg.rimworld.TD_Enhancement_Pack.main");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public static ThisMod Get() => LoadedModManager.GetMod<ThisMod>();
		public static HarmonyInstance Harmony() => Get().harmony;

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