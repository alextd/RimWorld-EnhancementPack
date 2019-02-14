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
		public bool dodgeGrenadeUnlessBelt = true;
		public bool showStopGizmo = true;
		public bool showStopGizmoDrafted = true;
		public bool cameraDragFixes = true;

		public bool showOverlayBuildable = true;
		public bool autoOverlayBuildable = true;
		public bool autoOverlaySmoothable = true;
		public bool autoOverlayTreeGrowth = true;
		public bool showOverlayPlantHarvest = true;
		public bool autoOverlayPlantHarvest = true;
		public bool autoOverlayWindBlocker = true;
		public bool autoOverlayLighting = true;
		public bool showOverlayBeauty = true;
		public bool showOverlayFertility = true;
		public bool autoOverlayFertility = true;
		public bool showOverlayWalkSpeed = true;
		public bool showOverlayLighting = true;
		public bool cheatFertilityUnderGrid = true;
		public float overlayOpacity = 1.0f;

		public bool changeSpeedAfterTrader = true;
		public int afterTraderSpeed = 0;

		public bool researchingArrow = true;

		public bool skillArrows = true;
		public bool skillUpArrows = true;
		public bool skillDownArrows = true;

		public bool showZoneSize = true;
		public bool fieldEdgesRedo = true;
		public bool zoneHarvestableToggle = false;
		public bool zoneRefill = false;
		
		public bool neverHome = true;
		public bool slaughterZone = true;

		public bool autorebuildDefaultOn = true;
		public bool caravanLoadSelection = true;
		public bool blueprintAnyStuff = true;
		public bool pawnTableHighlightSelected = true;
		public bool pawnTableClickSelect = false;
		public bool mouseoverInfoTopRight = false;

		public bool alertDeteriorating = true;
		public bool alertHeatstroke = true;
		public bool alertBurning = true;
		public bool alertNoBill = true;

		public bool matchGrowButton = true;
		public bool deepDrillRandom = true;

		public bool zoomToMouse = false;

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


			//Area / Zone things
			options.LabelHeader("TD.SettingsHeaderArea".Translate());
			options.CheckboxLabeled("TD.SettingMatchGrow".Translate(), ref matchGrowButton, "TD.SettingMatchGrowDesc".Translate());
			options.CheckboxLabeled("TD.SettingZoneSize".Translate(), ref showZoneSize, "TD.SettingZoneSizeDesc".Translate());
			options.CheckboxLabeled("TD.SettingAllowHarvesting".Translate(), ref zoneHarvestableToggle, "TD.SettingAllowHarvestingDesc".Translate());
			options.CheckboxLabeled("TD.NeverHome".Translate(), ref neverHome);
			options.CheckboxLabeled("TD.SlaughterZone".Translate(), ref slaughterZone);
			options.CheckboxLabeled("TD.SettingsCleanZone".Translate(), ref fieldEdgesRedo);
			options.Label("TD.AreaEditing".Translate());
			options.GapLine();


			//Overlays
			options.LabelHeader("TD.SettingsHeaderOverlays".Translate());
			options.CheckboxLabeled("TD.SettingOverlayBuildable".Translate(), ref showOverlayBuildable, "TD.SettingOverlayBuildableDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoBuildable".Translate(), ref autoOverlayBuildable, "TD.SettingAutoBuildableDesc".Translate());
			options.Gap();
			options.CheckboxLabeled("TD.SettingOverlayFertility".Translate(), ref showOverlayFertility);
			bool before = cheatFertilityUnderGrid;
			options.CheckboxLabeled("TD.SettingOverlayFertilityUnder".Translate(), ref cheatFertilityUnderGrid);
			if (before != cheatFertilityUnderGrid)
				BaseOverlay.SetDirty(typeof(FertilityOverlay));
			options.CheckboxLabeled("TD.SettingAutoFertility".Translate(), ref autoOverlayFertility);
			options.Gap();
			options.CheckboxLabeled("TD.SettingOverlayLighting".Translate(), ref showOverlayLighting, "TD.SettingOverlayLightingDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoLighting".Translate(), ref autoOverlayLighting);
			options.Gap();
			options.CheckboxLabeled("TD.SettingOverlayWalkSpeed".Translate(), ref showOverlayWalkSpeed);
			options.CheckboxLabeled("TD.SettingOverlayBeauty".Translate(), ref showOverlayBeauty);
			options.Gap();
			options.CheckboxLabeled("TD.SettingOverlayPlantHarvest".Translate(), ref showOverlayPlantHarvest);
			options.CheckboxLabeled("TD.SettingAutoPlantHarvest".Translate(), ref autoOverlayPlantHarvest);
			options.Gap();
			options.CheckboxLabeled("TD.SettingAutoSmoothable".Translate(), ref autoOverlaySmoothable);
			options.CheckboxLabeled("TD.SettingAutoTreeGrowth".Translate(), ref autoOverlayTreeGrowth, "TD.SettingAutoTreeGrowthDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoWindBlocker".Translate(), ref autoOverlayWindBlocker);
			options.Gap();
			float beforeO = overlayOpacity;
			options.SliderLabeled("TD.LowerOverlayOpacities".Translate(), ref overlayOpacity, "{0:P0}");
			if (beforeO != overlayOpacity)
				BaseOverlay.ResetAll();
			options.GapLine();


			//Game improvements
			options.LabelHeader("TD.SettingsHeaderGame".Translate());
			options.CheckboxLabeled("TD.DeepDrillRandomrock".Translate(), ref deepDrillRandom, "TD.DeepDrillRandomrockDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoAutorebuild".Translate(), ref autorebuildDefaultOn);
			options.CheckboxLabeled("TD.CaravanLoadSelection".Translate(), ref caravanLoadSelection);
			options.CheckboxLabeled("TD.BlueprintAnyStuff".Translate(), ref blueprintAnyStuff);
			options.Label("TD.SarcophagusPreferred".Translate(), tooltip: "TD.SarcophagusPreferredDesc".Translate());
			options.GapLine();


			//Notifications / Info
			options.LabelHeader("TD.SettingsHeaderInfo".Translate());
			options.CheckboxLabeled("TD.SettingPawnTableHighlightSelected".Translate(), ref pawnTableHighlightSelected);
			options.CheckboxLabeled("TD.SettingPawnTableClickSelect".Translate(), ref pawnTableClickSelect, "TD.SettingPawnTableClickSelectDesc".Translate());
			options.CheckboxLabeled("TD.SettingTopRightMouseover".Translate(), ref mouseoverInfoTopRight, "TD.SettingTopRightMouseoverDesc".Translate());
			options.CheckboxLabeled("TD.SettingAlertDeterioration".Translate(), ref alertDeteriorating, "TD.SettingAlertDeteriorationDesc".Translate());
			options.CheckboxLabeled("TD.SettingAlertHeatstroke".Translate(), ref alertHeatstroke);
			options.CheckboxLabeled("TD.SettingAlertBurning".Translate(), ref alertBurning);
			options.CheckboxLabeled("TD.SettingAlertNoBill".Translate(), ref alertNoBill);
			options.CheckboxLabeled("TD.SettingTradeClose".Translate(), ref changeSpeedAfterTrader);
			options.SliderLabeled("TD.SettingTradeCloseSpeed".Translate(), ref afterTraderSpeed, "{0}x", 0, 4);
			options.Gap();
			options.CheckboxLabeled("TD.SettingResearchingArrow".Translate(), ref researchingArrow);
			options.CheckboxLabeled("TD.SettingSkillGainArrow".Translate(), ref skillUpArrows, "TD.SettingSkillGainArrowDesc".Translate());
			options.CheckboxLabeled("TD.SettingSkillLossArrow".Translate(), ref skillDownArrows, "TD.SettingSkillLossArrowDesc".Translate());
			skillArrows = skillUpArrows || skillDownArrows;
			options.Gap();
			options.Label("TD.FeatureConditionGreen".Translate());
			options.Label("TD.DropPodWhatDropped".Translate());
			options.GapLine();


			//AI
			options.LabelHeader("TD.SettingsHeaderAI".Translate());
			options.CheckboxLabeled("TD.SettingsIgnoreSleeping".Translate(), ref ignoreSleepingEnemies, "TD.SettingsIgnoreSleepingDesc".Translate());
			options.CheckboxLabeled("TD.SettingStopFlee".Translate(), ref stopFlee, "TD.SettingStopFleeDesc".Translate());
			options.CheckboxLabeled("TD.SettingDodgeGrenades".Translate(), ref dodgeGrenade, "TD.SettingDodgeGrenadesDesc".Translate());
			options.CheckboxLabeled("TD.SettingDodgeGrenadesUnlessBelt".Translate(), ref dodgeGrenadeUnlessBelt);
			options.Gap();
			options.CheckboxLabeled("TD.ShowStopButtonDrafted".Translate(), ref showStopGizmoDrafted);
			options.CheckboxLabeled("TD.ShowStopButtonUnDrafted".Translate(), ref showStopGizmo);
			options.GapLine();


			options.Label("TD.RequiresRestart".Translate());
			options.CheckboxLabeled("TD.SettingUrgentRefill".Translate(), ref zoneRefill, "TD.SettingUrgentRefillDesc".Translate());
			options.CheckboxLabeled("TD.SettingCameraDragFix".Translate(), ref cameraDragFixes, "TD.SettingCameraDragFixDesc".Translate());
			options.GapLine();

			options.Label("TD.OtherFeatures".Translate());
			options.CheckboxLabeled("Zoom to Mouse", ref zoomToMouse);
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
			Scribe_Values.Look(ref dodgeGrenadeUnlessBelt, "dodgeGrenadeUnlessBelt", true);
			Scribe_Values.Look(ref showStopGizmo, "showStopGizmo", true);
			Scribe_Values.Look(ref showStopGizmoDrafted, "showStopGizmoDrafted", true);
			Scribe_Values.Look(ref cameraDragFixes, "cameraDragFixes", true);

			Scribe_Values.Look(ref showOverlayBuildable, "showOverlayBuildable", true);
			Scribe_Values.Look(ref autoOverlayBuildable, "autoOverlayBuildable", true);
			Scribe_Values.Look(ref autoOverlaySmoothable, "autoOverlaySmoothable", true);
			Scribe_Values.Look(ref autoOverlayTreeGrowth, "autoOverlayTreeGrowth", true);
			Scribe_Values.Look(ref showOverlayPlantHarvest, "showOverlayPlantHarvest", true);
			Scribe_Values.Look(ref autoOverlayPlantHarvest, "autoOverlayPlantHarvest", true);
			Scribe_Values.Look(ref autoOverlayWindBlocker, "autoOverlayWindBlocker", true);
			Scribe_Values.Look(ref autoOverlayLighting, "autoOverlayLighting", true);
			Scribe_Values.Look(ref showOverlayBeauty, "showOverlayBeauty", true);
			Scribe_Values.Look(ref showOverlayFertility, "showOverlayFertility", true);
			Scribe_Values.Look(ref autoOverlayFertility, "autoOverlayFertility", true);
			Scribe_Values.Look(ref showOverlayWalkSpeed, "showOverlayWalkSpeed", true);
			Scribe_Values.Look(ref showOverlayLighting, "showOverlayLighting", true);
			Scribe_Values.Look(ref cheatFertilityUnderGrid, "cheatFertilityUnderGrid", true);
			Scribe_Values.Look(ref overlayOpacity, "overlayOpacity", 1.0f);

			Scribe_Values.Look(ref changeSpeedAfterTrader, "changeSpeedAfterTrader", true);
			Scribe_Values.Look(ref afterTraderSpeed, "afterTraderSpeed", 0);

			Scribe_Values.Look(ref researchingArrow, "researchingArrow", true);
			Scribe_Values.Look(ref skillArrows, "skillArrows", true);
			Scribe_Values.Look(ref skillUpArrows, "skillUpArrows", true);
			Scribe_Values.Look(ref skillDownArrows, "skillDownArrows", true);

			Scribe_Values.Look(ref showZoneSize, "showZoneSize", true);
			Scribe_Values.Look(ref fieldEdgesRedo, "fieldEdgesRedo", true);
			Scribe_Values.Look(ref zoneHarvestableToggle, "zoneHarvestableToggle", false);
			Scribe_Values.Look(ref zoneRefill, "zoneRefill", true);
			
			Scribe_Values.Look(ref neverHome, "neverHome", true);
			Scribe_Values.Look(ref slaughterZone, "slaughterZone", true);

			Scribe_Values.Look(ref autorebuildDefaultOn, "autorebuildDefaultOn", true);
			Scribe_Values.Look(ref caravanLoadSelection, "caravanLoadSelection", true);
			Scribe_Values.Look(ref blueprintAnyStuff, "blueprintAnyStuff", true);
			Scribe_Values.Look(ref pawnTableHighlightSelected, "pawnTableHighlightSelected", true);
			Scribe_Values.Look(ref pawnTableClickSelect, "pawnTableClickSelect", false);
			Scribe_Values.Look(ref mouseoverInfoTopRight, "mouseoverInfoTopRight", false);

			Scribe_Values.Look(ref alertDeteriorating, "alertDeteriorating", true);
			Scribe_Values.Look(ref alertHeatstroke, "alertHeatstroke", true);
			Scribe_Values.Look(ref alertBurning, "alertBurning", true);
			Scribe_Values.Look(ref alertNoBill, "alertNoBill", true);

			Scribe_Values.Look(ref matchGrowButton, "matchGrowButton", true);
			Scribe_Values.Look(ref deepDrillRandom, "deepDrillRandom", true);

			Scribe_Values.Look(ref zoomToMouse, "zoomToMouse", false);
		}
	}
}