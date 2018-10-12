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
		public bool ignoreSleepingEnemies = true;
		public bool stopFlee = true;
		public bool dodgeGrenade = false;
		public bool showStopGizmo = true;
		public bool showStopGizmoDrafted = true;

		public bool showOverlayBuildable = true;
		public bool showOverlayBeauty = true;
		public bool showOverlayFertility = true;
		public bool autoOverlayFertility = true;
		public bool showOverlayLighting = true;
		public bool cheatFertilityUnderGrid = true;

		public bool changeSpeedAfterTrader = true;
		public int afterTraderSpeed = 0;

		public bool skillArrows = true;
		public bool skillUpArrows = true;
		public bool skillDownArrows = true;

		public bool showZoneSize = true;
		public bool zoneHarvestableToggle = false;
		
		public bool neverHome = true;
		public bool slaughterZone = true;

		public bool autorebuildDefaultOn = true;
		public bool mouseoverInfoTopRight = false;
		public bool alertDeteriorating = true;
		public bool matchGrowButton = true;

		public Vector2 scrollPosition;
		public float scrollViewHeight;

		public static Settings Get()
		{
			return LoadedModManager.GetMod<Mod>().GetSettings<Settings>();
		}

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			
			Rect viewRect = new Rect(0f, 0f, wrect.width - 16f, scrollViewHeight);
			options.BeginScrollViewEx(wrect, ref scrollPosition, ref viewRect);
			
			options.CheckboxLabeled("TD.SettingsIgnoreSleeping".Translate(), ref ignoreSleepingEnemies, "TD.SettingsIgnoreSleepingDesc".Translate());
			options.CheckboxLabeled("Stop fleeing if threat is gone", ref stopFlee, "Pretty sure this works\n\nSetting change doesn't apply to people already fleeing");
			options.CheckboxLabeled("Dodge grenades while drafted", ref dodgeGrenade, "Of course this interrupts what you're doing.");
			options.CheckboxLabeled("TD.ShowStopButtonDrafted".Translate(), ref showStopGizmoDrafted);
			options.CheckboxLabeled("TD.ShowStopButtonUnDrafted".Translate(), ref showStopGizmo);
			options.Gap();
			
			options.CheckboxLabeled("TD.SettingOverlayBuildable".Translate(), ref showOverlayBuildable, "TD.SettingOverlayBuildableDesc".Translate());
			options.CheckboxLabeled("TD.SettingOverlayBeauty".Translate(), ref showOverlayBeauty, "I know it slows down the game");
			options.CheckboxLabeled("TD.SettingOverlayFertility".Translate(), ref showOverlayFertility);
			bool before = cheatFertilityUnderGrid;
			options.CheckboxLabeled("TD.SettingOverlayFertilityUnder".Translate(), ref cheatFertilityUnderGrid);
			if (before != cheatFertilityUnderGrid)
				FertilityOverlay.DirtyAll();
			options.CheckboxLabeled("Auto-show fertility overlay when placing growing zones", ref autoOverlayFertility);
			options.CheckboxLabeled("TD.SettingOverlayLighting".Translate(), ref showOverlayLighting, "TD.SettingOverlayLightingDesc".Translate());
			options.Gap();

			options.CheckboxLabeled("TD.SettingTradeClose".Translate(), ref changeSpeedAfterTrader);
			options.SliderLabeled("TD.SettingTradeCloseSpeed".Translate(), ref afterTraderSpeed, "{0}x", 0, 4);
			options.Gap();

			options.CheckboxLabeled("TD.SettingSkillGainArrow".Translate(), ref skillUpArrows, "TD.SettingSkillGainArrowDesc".Translate());
			options.CheckboxLabeled("TD.SettingSkillLossArrow".Translate(), ref skillDownArrows, "TD.SettingSkillLossArrowDesc".Translate());
			skillArrows = skillUpArrows || skillDownArrows;
			options.Gap();

			options.CheckboxLabeled("TD.SettingZoneSize".Translate(), ref showZoneSize);
			options.CheckboxLabeled("TD.SettingAllowHarvesting".Translate(), ref zoneHarvestableToggle);
			options.Gap();

			options.CheckboxLabeled("TD.NeverHome".Translate(), ref neverHome);
			options.CheckboxLabeled("TD.SlaughterZone".Translate(), ref slaughterZone);
			options.Gap();

			options.CheckboxLabeled("Autorebuild is on for a new game", ref autorebuildDefaultOn);
			options.CheckboxLabeled("Mouseover tile info is on topright", ref mouseoverInfoTopRight, "So it shows up when something is selected\n\nOf course this is a bad idea if you still have the learning helper on.");
			options.CheckboxLabeled("Alert for deteriorating things", ref alertDeteriorating, "Deteriorating things with less than 50% HP, that are not forbidden");
			options.CheckboxLabeled("Button to make a growing zone and match terrain", ref matchGrowButton, "Terrain is only added if fertility % matches the first tile selected when dragging");

			options.Label("TD.RequiresRestart".Translate());
			options.Gap();

			options.Label("TD.OtherFeatures".Translate());
			options.Label("TD.AreaEditing".Translate());
			options.Label("TD.FeatureConditionGreen".Translate());
			options.Label("A debug menu action to place a full stack of things");
			options.Label("The letter for random resource drop pods tell you what dropped");
			options.Label("Sarcophagus preferred to use over graves", tooltip: "The default settings for a Sarcophagus are Critical so they are used before graves. The settings are already colonist-only, but without setting Critical, colonists would be buried in closer graves instead");
			options.Label("With debug godmode on, roofs are added/removed instantly, and walls/floors are smoothed instantly");
			options.Label("Frames can't be deconstructed, only canceled", tooltip: "Deconstruction would return less resources than canceling");
			options.Label("Dropdown designators get their order set by their contained designators", tooltip: "e.g. colored lights show up in the build menu next to other lights instead of the front of the list");
			options.Label("Selected stone has a button to smooth it");
			options.Label("Deep drilling for rock gets a random rock type", tooltip: "Would normally only get one type");

			options.EndScrollView(ref viewRect);
			scrollViewHeight = viewRect.height;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref ignoreSleepingEnemies, "ignoreSleepingEnemies", true);
			Scribe_Values.Look(ref stopFlee, "stopFlee", true);
			Scribe_Values.Look(ref dodgeGrenade, "dodgeGrenade", false);
			Scribe_Values.Look(ref showStopGizmo, "showStopGizmo", true);
			Scribe_Values.Look(ref showStopGizmoDrafted, "showStopGizmoDrafted", true);

			Scribe_Values.Look(ref showOverlayBuildable, "showOverlayBuildable", true);
			Scribe_Values.Look(ref showOverlayBeauty, "showOverlayBeauty", true);
			Scribe_Values.Look(ref showOverlayFertility, "showOverlayFertility", true);
			Scribe_Values.Look(ref autoOverlayFertility, "autoOverlayFertility", true);
			Scribe_Values.Look(ref showOverlayLighting, "showOverlayLighting", true);
			Scribe_Values.Look(ref cheatFertilityUnderGrid, "cheatFertilityUnderGrid", true);

			Scribe_Values.Look(ref changeSpeedAfterTrader, "changeSpeedAfterTrader", true);
			Scribe_Values.Look(ref afterTraderSpeed, "afterTraderSpeed", 0);

			Scribe_Values.Look(ref skillArrows, "skillArrows", true);
			Scribe_Values.Look(ref skillUpArrows, "skillUpArrows", true);
			Scribe_Values.Look(ref skillDownArrows, "skillDownArrows", true);

			Scribe_Values.Look(ref showZoneSize, "showZoneSize", true);
			Scribe_Values.Look(ref zoneHarvestableToggle, "zoneHarvestableToggle", false);
			
			Scribe_Values.Look(ref neverHome, "neverHome", true);
			Scribe_Values.Look(ref slaughterZone, "slaughterZone", true);

			Scribe_Values.Look(ref autorebuildDefaultOn, "autorebuildDefaultOn", true);
			Scribe_Values.Look(ref mouseoverInfoTopRight, "mouseoverInfoTopRight", false);
			Scribe_Values.Look(ref alertDeteriorating, "alertDeteriorating", true);
			Scribe_Values.Look(ref matchGrowButton, "matchGrowButton", true);
		}
	}
}