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
		public bool autoOverlayBuildable = true;
		public bool autoOverlaySmoothable = true;
		public bool autoOverlayWindBlocker = true;
		public bool showOverlayBeauty = true;
		public bool showOverlayFertility = true;
		public bool autoOverlayFertility = true;
		public bool showOverlayWalkSpeed = true;
		public bool showOverlayLighting = true;
		public bool cheatFertilityUnderGrid = true;
		public float overlayOpacity = 1.0f;

		public bool changeSpeedAfterTrader = true;
		public int afterTraderSpeed = 0;

		public bool skillArrows = true;
		public bool skillUpArrows = true;
		public bool skillDownArrows = true;

		public bool showZoneSize = true;
		public bool zoneHarvestableToggle = false;
		public bool zoneRefill = false;
		
		public bool neverHome = true;
		public bool slaughterZone = true;

		public bool autorebuildDefaultOn = true;
		public bool mouseoverInfoTopRight = false;
		public bool alertDeteriorating = true;
		public bool matchGrowButton = true;
		public bool deepDrillRandom = true;

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
			options.CheckboxLabeled("TD.SettingStopFlee".Translate(), ref stopFlee, "TD.SettingStopFleeDesc".Translate());
			options.CheckboxLabeled("TD.SettingDodgeGrenades".Translate(), ref dodgeGrenade, "TD.SettingDodgeGrenadesDesc".Translate());
			options.CheckboxLabeled("TD.ShowStopButtonDrafted".Translate(), ref showStopGizmoDrafted);
			options.CheckboxLabeled("TD.ShowStopButtonUnDrafted".Translate(), ref showStopGizmo);
			options.Gap();
			
			options.CheckboxLabeled("TD.SettingOverlayBuildable".Translate(), ref showOverlayBuildable, "TD.SettingOverlayBuildableDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoBuildable".Translate(), ref autoOverlayBuildable, "TD.SettingAutoBuildableDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoSmoothable".Translate(), ref autoOverlaySmoothable);
			options.CheckboxLabeled("TD.SettingAutoWindBlocker".Translate(), ref autoOverlayWindBlocker);
			options.CheckboxLabeled("TD.SettingOverlayBeauty".Translate(), ref showOverlayBeauty, "TD.SettingBeautySlow".Translate());
			options.CheckboxLabeled("TD.SettingOverlayFertility".Translate(), ref showOverlayFertility);
			bool before = cheatFertilityUnderGrid;
			options.CheckboxLabeled("TD.SettingOverlayFertilityUnder".Translate(), ref cheatFertilityUnderGrid);
			if (before != cheatFertilityUnderGrid)
				FertilityOverlay.DirtyAll();
			options.CheckboxLabeled("TD.SettingAutoFertility".Translate(), ref autoOverlayFertility);
			options.CheckboxLabeled("TD.SettingOverlayWalkSpeed".Translate(), ref showOverlayWalkSpeed);
			options.CheckboxLabeled("TD.SettingOverlayLighting".Translate(), ref showOverlayLighting, "TD.SettingOverlayLightingDesc".Translate());
			float beforeO = overlayOpacity;
			options.SliderLabeled("TD.LowerOverlayOpacities".Translate(), ref overlayOpacity, "{0:P0}");
			if (beforeO != overlayOpacity)
				BaseOverlay.SetAllOpacity(overlayOpacity);
			options.Gap();

			options.CheckboxLabeled("TD.SettingTradeClose".Translate(), ref changeSpeedAfterTrader);
			options.SliderLabeled("TD.SettingTradeCloseSpeed".Translate(), ref afterTraderSpeed, "{0}x", 0, 4);
			options.Gap();

			options.CheckboxLabeled("TD.SettingSkillGainArrow".Translate(), ref skillUpArrows, "TD.SettingSkillGainArrowDesc".Translate());
			options.CheckboxLabeled("TD.SettingSkillLossArrow".Translate(), ref skillDownArrows, "TD.SettingSkillLossArrowDesc".Translate());
			skillArrows = skillUpArrows || skillDownArrows;
			options.Gap();

			options.CheckboxLabeled("TD.SettingZoneSize".Translate(), ref showZoneSize);
			options.CheckboxLabeled("TD.SettingAllowHarvesting".Translate(), ref zoneHarvestableToggle, "TD.SettingAllowHarvestingDesc".Translate());
			options.Gap();

			options.CheckboxLabeled("TD.NeverHome".Translate(), ref neverHome);
			options.CheckboxLabeled("TD.SlaughterZone".Translate(), ref slaughterZone);
			options.Gap();

			options.CheckboxLabeled("TD.SettingAutoAutorebuild".Translate(), ref autorebuildDefaultOn);
			options.CheckboxLabeled("TD.SettingTopRightMouseover".Translate(), ref mouseoverInfoTopRight, "TD.SettingTopRightMouseoverDesc".Translate());
			options.CheckboxLabeled("TD.SettingDeteriorationAlert".Translate(), ref alertDeteriorating, "TD.SettingDeteriorationAlertDesc".Translate());
			options.CheckboxLabeled("TD.SettingMatchGrow".Translate(), ref matchGrowButton, "TD.SettingMatchGrowDesc".Translate());
			options.CheckboxLabeled("TD.DeepDrillRandomrock".Translate(), ref deepDrillRandom, "TD.DeepDrillRandomrockDesc".Translate());
			options.Gap();

			options.Label("TD.RequiresRestart".Translate());
			options.CheckboxLabeled("TD.SettingUrgentRefill".Translate(), ref zoneRefill, "TD.SettingUrgentRefillDesc".Translate());
			options.GapLine();

			options.Label("TD.OtherFeatures".Translate());
			options.Label("TD.AreaEditing".Translate());
			options.Label("TD.FeatureConditionGreen".Translate());
			options.Label("TD.DebugFullStack".Translate());
			options.Label("TD.DropPodWhatDropped".Translate());
			options.Label("TD.SarcophagusPreferred".Translate(), tooltip: "TD.SarcophagusPreferredDesc".Translate());
			options.Label("TD.DebugGodmodeRoofFloors".Translate());
			options.Label("TD.NoFrameDecon".Translate(), tooltip: "TD.NoFrameDeconDesc".Translate());
			options.Label("TD.DropdownBuildingsOrder".Translate(), tooltip: "TD.DropdownBuildingsOrderDesc".Translate());

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
			Scribe_Values.Look(ref autoOverlayBuildable, "autoOverlayBuildable", true);
			Scribe_Values.Look(ref autoOverlaySmoothable, "autoOverlaySmoothable", true);
			Scribe_Values.Look(ref autoOverlayWindBlocker, "autoOverlayWindBlocker", true);
			Scribe_Values.Look(ref showOverlayBeauty, "showOverlayBeauty", true);
			Scribe_Values.Look(ref showOverlayFertility, "showOverlayFertility", true);
			Scribe_Values.Look(ref autoOverlayFertility, "autoOverlayFertility", true);
			Scribe_Values.Look(ref showOverlayWalkSpeed, "showOverlayWalkSpeed ", true);
			Scribe_Values.Look(ref showOverlayLighting, "showOverlayLighting", true);
			Scribe_Values.Look(ref cheatFertilityUnderGrid, "cheatFertilityUnderGrid", true);
			Scribe_Values.Look(ref overlayOpacity, "overlayOpacity", 1.0f);

			Scribe_Values.Look(ref changeSpeedAfterTrader, "changeSpeedAfterTrader", true);
			Scribe_Values.Look(ref afterTraderSpeed, "afterTraderSpeed", 0);

			Scribe_Values.Look(ref skillArrows, "skillArrows", true);
			Scribe_Values.Look(ref skillUpArrows, "skillUpArrows", true);
			Scribe_Values.Look(ref skillDownArrows, "skillDownArrows", true);

			Scribe_Values.Look(ref showZoneSize, "showZoneSize", true);
			Scribe_Values.Look(ref zoneHarvestableToggle, "zoneHarvestableToggle", false);
			Scribe_Values.Look(ref zoneRefill, "zoneRefill", true);
			
			Scribe_Values.Look(ref neverHome, "neverHome", true);
			Scribe_Values.Look(ref slaughterZone, "slaughterZone", true);

			Scribe_Values.Look(ref autorebuildDefaultOn, "autorebuildDefaultOn", true);
			Scribe_Values.Look(ref mouseoverInfoTopRight, "mouseoverInfoTopRight", false);
			Scribe_Values.Look(ref alertDeteriorating, "alertDeteriorating", true);
			Scribe_Values.Look(ref matchGrowButton, "matchGrowButton", true);
			Scribe_Values.Look(ref deepDrillRandom, "deepDrillRandom", true);
		}
	}
}