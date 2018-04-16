using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using TD.Utilities;

namespace TD_Enhancement_Pack
{
	class Settings : ModSettings
	{
		public bool showOverlayBuildable = true;
		public bool showOverlayFertility = true;
		public bool showOverlayLighting = true;
		public bool cheatFertilityUnderGrid = true;

		public bool changeSpeedAfterTrader = true;
		public int afterTraderSpeed = 0;

		public bool skillArrows = true;
		public bool skillUpArrows = true;
		public bool skillDownArrows = true;

		public bool showZoneSize = true;

		public static Settings Get()
		{
			return LoadedModManager.GetMod<TD_Enhancement_Pack.Mod>().GetSettings<Settings>();
		}

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.Begin(wrect);
			options.CheckboxLabeled("Enable a buildable terrain overlay", ref showOverlayBuildable, "Red is unbuildable, Yellow is 'Light' Buildable (e.g. not structures mostly)");
			options.CheckboxLabeled("Enable a fertility overlay", ref showOverlayFertility);
			bool before = cheatFertilityUnderGrid;
			options.CheckboxLabeled("Fertility overlay shows value for terrain under floors", ref cheatFertilityUnderGrid);
			if (before != cheatFertilityUnderGrid)
				FertilityOverlay.DirtyAll();
			options.CheckboxLabeled("Enable a lighting overlay", ref showOverlayLighting, "Sun-lit outdoor areas are uncolored");
			options.Gap();

			options.CheckboxLabeled("Set speed after closing trade or negotiation dialog", ref changeSpeedAfterTrader);
			options.SliderLabeled("Speed after closing: ", ref afterTraderSpeed, "{0}x", 0, 4);
			options.Gap();

			options.CheckboxLabeled("Show arrows indicating skill gain", ref skillUpArrows, "In the Character Tab");
			options.CheckboxLabeled("Show arrows indicating skill loss", ref skillDownArrows, "Skills slowly lose experience after level 10 (The arrow can be very faded for slow rates)");
			skillArrows = skillUpArrows || skillDownArrows;
			options.Gap();

			options.CheckboxLabeled("Show Stockpile/Growing zone area size and info", ref showZoneSize);
			options.Gap();

			options.Label("Other unanimously good features so they don't get option buttons:");
			options.Label("Areas can be reordered, recolored, and copy/pasted in the area manager");
			options.Label("Camera Panning at low framerates is fixed: Panning moves and slows to a stop at real-time speed instead of game framerate");
			options.Label("Make an area named 'Never Home' and the area will never be added to the Home Area");
			options.Label("(various) selected items are given a count");
			options.Label("Food 'rotted away' messages are clickable to see where it was");
			options.Label("Popup messages like '% recruit chance' (text motes) stay onscreen in realtime, so they don't dissappear at high speeds");

			options.End();
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref showOverlayBuildable, "showOverlayBuildable", true);
			Scribe_Values.Look(ref showOverlayFertility, "showOverlayFertility", true);
			Scribe_Values.Look(ref showOverlayLighting, "showOverlayLighting", true);
			Scribe_Values.Look(ref cheatFertilityUnderGrid, "cheatFertilityUnderGrid", true);

			Scribe_Values.Look(ref changeSpeedAfterTrader, "changeSpeedAfterTrader", true);
			Scribe_Values.Look(ref afterTraderSpeed, "afterTraderSpeed", 0);

			Scribe_Values.Look(ref skillArrows, "skillArrows", true);
			Scribe_Values.Look(ref skillUpArrows, "skillUpArrows", true);
			Scribe_Values.Look(ref skillDownArrows, "skillDownArrows", true);

			Scribe_Values.Look(ref showZoneSize, "showZoneSize", true);
		}
	}
}