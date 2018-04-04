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
			// initialize settings
			// GetSettings<Settings>();
#if DEBUG
			HarmonyInstance.DEBUG = true;
#endif
			HarmonyInstance harmony = HarmonyInstance.Create("Uuugggg.rimworld.TD_Enhancement_Pack.main");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			harmony.Patch(AccessTools.Constructor(typeof(Dialog_ManageAreas), new Type[] { typeof(Map)}),
				null, new HarmonyMethod(typeof(Dialog_ManageAreas_Patch), "Postfix"));
			harmony.Patch(AccessTools.Property(typeof(Area_Allowed), nameof(Area_Allowed.ListPriority)).GetGetMethod(false),
				null, new HarmonyMethod(typeof(AreaOrder), nameof(AreaOrder.ListPriority_Postfix)));
		}


		// public override void DoSettingsWindowContents(Rect inRect)
		// {
		// base.DoSettingsWindowContents(inRect);
		// GetSettings<Settings>().DoWindowContents(inRect);
		// }

		// public override string SettingsCategory()
		// {
		// return "TDEnhancementPack".Translate();
		// }
	}
}