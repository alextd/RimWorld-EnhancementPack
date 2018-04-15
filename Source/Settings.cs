using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace TD_Enhancement_Pack
{
	class Settings : ModSettings
	{
		public bool cheatFertilityUnderGrid;

		public static Settings Get()
		{
			return LoadedModManager.GetMod<TD_Enhancement_Pack.Mod>().GetSettings<Settings>();
		}

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.Begin(wrect);
			
			options.CheckboxLabeled("Fertility overlay shows for terrain under floors", ref cheatFertilityUnderGrid);
			options.Gap();

			options.End();
		}
		
		public override void ExposeData()
		{
			Scribe_Values.Look(ref cheatFertilityUnderGrid, "cheatFertilityUnderGrid", true);
		}
	}
}