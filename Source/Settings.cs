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
		public bool billCountEquippedAny = true;

		public bool showOverlayBuildable = true;
		public bool autoOverlayBuildable = true;
		public bool showOverlayCoverage = true;
		public bool autoOverlayCoverage = true;
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
		public bool showOverlayPower = true;
		public bool cheatFertilityUnderGrid = true;
		public float overlayOpacity = 1.0f;

		public bool changeSpeedAfterTrader = true;
		public int afterTraderSpeed = 0;

		public bool researchingArrow = true;

		public bool skillArrows = true;
		public bool skillUpArrows = true;
		public bool skillDownArrows = true;

		public bool dropPodWhatDropped = true;

		public bool showZoneSize = true;
		public bool fieldEdgesRedo = true;
		public bool zoneHarvestableToggle = false;
		public bool areaForTypes = true;
		public bool zoneRefill = false;
		
		public bool neverHome = true;
		public bool slaughterZone = true;

		public bool autorebuildDefaultOn = true;
		public bool autoRebuildTransportPod = false;
		public bool caravanLoadSelection = true;
		public bool caravanSaveManifest = true;
		public bool tradeRequestWorstFirst = true;
		public bool blueprintAnyStuff = true;
		public bool copyPolicyButton = false;

		public bool pawnTableHighlightSelected = true;
		public bool pawnTableArrowMouseover = true;
		public bool rememberResourceReadout = true;
		public bool startOpenResourceReadout = false;
		public bool pawnTableClickSelect = false;
		public bool selectedItemsZoomButton = true;
		public bool mouseoverInfoTopRight = false;
		public bool stopForcedSlowdown = true;

		public bool alertDeteriorating = true;
		public bool alertHeatstroke = true;
		public bool alertBurning = true;
		public bool alertToxic = true;
		public bool alertNoBill = true;
		public bool alertWindBlocker = true;

		public bool areasUnlimited = true;
		public bool matchGrowButton = true;
		public bool deepDrillRandom = true;

		public bool zoomToMouse = false;

		public bool showToggleLearning = true;
		public bool showToggleZone = true;
		public bool showToggleBeauty = true;
		public bool showToggleRoomstats = true;
		public bool showToggleColonists = true;
		public bool showToggleRoof = true;
		public bool showToggleHomeArea = true;
		public bool showToggleRebuild = true;
		public bool showToggleCategorized = true;

		public bool colorVariation = false;
		public bool colorGenerator = false;
		public float colorGenChance = 0.5f;
		public bool colorFixStuffColor = false;
		public bool colorFixDominant = false;
		public bool colorRedoWarned = false;


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
			options.BeginScrollViewEx(wrect, ref scrollPosition, viewRect);


			//Area / Zone things
			options.LabelHeader("TD.SettingsHeaderArea".Translate());
			options.CheckboxLabeled("TD.NoLimitOnTheNumberOfAreas".Translate(), ref areasUnlimited);
			options.CheckboxLabeled("TD.SettingMatchGrow".Translate(), ref matchGrowButton, "TD.SettingMatchGrowDesc".Translate());
			options.CheckboxLabeled("TD.SettingZoneSize".Translate(), ref showZoneSize, "TD.SettingZoneSizeDesc".Translate());
			options.CheckboxLabeled("TD.SettingAllowHarvesting".Translate(), ref zoneHarvestableToggle, "TD.SettingAllowHarvestingDesc".Translate());
			options.CheckboxLabeled("TD.SettingAreaForTypes".Translate(), ref areaForTypes, "TD.SettingAreaForTypesDesc".Translate());
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
			options.CheckboxLabeled("TD.SettingOverlayCoverage".Translate(), ref showOverlayCoverage, "TD.SettingOverlayCoverageDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoCoverage".Translate(), ref autoOverlayCoverage, "TD.SettingAutoCoverageDesc".Translate());
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
			options.CheckboxLabeled("TD.SettingOverlayPower".Translate(), ref showOverlayPower);
			options.CheckboxLabeled("TD.SettingAutoSmoothable".Translate(), ref autoOverlaySmoothable);
			options.CheckboxLabeled("TD.SettingAutoTreeGrowth".Translate(), ref autoOverlayTreeGrowth, "TD.SettingAutoTreeGrowthDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoWindBlocker".Translate(), ref autoOverlayWindBlocker);
			options.Gap();
			float beforeO = overlayOpacity;
			options.SliderLabeled("TD.LowerOverlayOpacities".Translate(), ref overlayOpacity, "{0:P0}");
			if (beforeO != overlayOpacity)
				BaseOverlay.ResetAll();
			options.GapLine();


			//Hide bottom-right Toggleable buttons
			options.LabelHeader("TD.SettingHeaderToggleButtons".Translate());
			options.Label("TD.SettingHeaderToggleButtonsDesc".Translate());
			options.CheckboxLabeled("TD.Show".Translate("ShowLearningHelperWhenEmptyToggleButton".Translate().Split('\n')[0]), ref showToggleLearning);
			options.CheckboxLabeled("TD.Show".Translate("ZoneVisibilityToggleButton".Translate().Split('\n')[0]), ref showToggleZone);
			options.CheckboxLabeled("TD.Show".Translate("ShowBeautyToggleButton".Translate().Split('\n')[0]), ref showToggleBeauty);
			options.CheckboxLabeled("TD.Show".Translate("ShowRoomStatsToggleButton".Translate().Split('\n')[0]), ref showToggleRoomstats);
			options.CheckboxLabeled("TD.Show".Translate("ShowColonistBarToggleButton".Translate().Split('\n')[0]), ref showToggleColonists);
			options.CheckboxLabeled("TD.Show".Translate("ShowRoofOverlayToggleButton".Translate().Split('\n')[0]), ref showToggleRoof);
			options.CheckboxLabeled("TD.Show".Translate("AutoHomeAreaToggleButton".Translate().Split('\n')[0]), ref showToggleHomeArea);
			options.CheckboxLabeled("TD.Show".Translate("AutoRebuildButton".Translate().Split('\n')[0]), ref showToggleRebuild);
			options.CheckboxLabeled("TD.Show".Translate("CategorizedResourceReadoutToggleButton".Translate().Split('\n')[0]), ref showToggleCategorized);
			options.GapLine();


			//Game improvements
			options.LabelHeader("TD.SettingsHeaderGame".Translate());
			options.CheckboxLabeled("TD.DeepDrillRandomrock".Translate(), ref deepDrillRandom, "TD.DeepDrillRandomrockDesc".Translate());
			options.CheckboxLabeled("TD.SettingAutoAutorebuild".Translate(), ref autorebuildDefaultOn);
			options.CheckboxLabeled("TD.SettingAutoRebuildTransportPod".Translate(), ref autoRebuildTransportPod);
			options.Gap();
			options.CheckboxLabeled("TD.CaravanLoadSelection".Translate(), ref caravanLoadSelection);
			options.CheckboxLabeled("TD.CaravanSaveManifest".Translate(), ref caravanSaveManifest);
			options.Label("TD.CaravanDesc".Translate());
			options.CheckboxLabeled("TD.CaravanWorstFirst".Translate(), ref tradeRequestWorstFirst, "TD.CaravanWorstFirstDesc".Translate());
			options.Gap();
			options.CheckboxLabeled("TD.BlueprintAnyStuff".Translate(), ref blueprintAnyStuff);
			options.CheckboxLabeled("TD.SettingCopyPolicy".Translate(), ref copyPolicyButton);
			options.CheckboxLabeled("TD.SettingBillCountEquippedAny".Translate(), ref billCountEquippedAny, "TD.SettingBillCountEquippedAnyDesc".Translate());
			options.Label("TD.SarcophagusPreferred".Translate(), tooltip: "TD.SarcophagusPreferredDesc".Translate());
			options.GapLine();


			//Color variations
			options.LabelHeader("TD.SettingHeaderColorVariations".Translate());
			options.CheckboxLabeled("TD.SettingColorVariation".Translate(), ref colorVariation, "TD.SettingColorVariationDesc".Translate());
			options.CheckboxLabeled("TD.SettingColorGenerator".Translate(), ref colorGenerator, "TD.SettingColorGeneratorDesc".Translate());
			options.SliderLabeled("TD.SettingColorGeneratorChance".Translate(), ref colorGenChance, "{0:P0}", 0, 1);
			options.Gap();

			options.CheckboxLabeled("TD.SettingFixColorStuff".Translate(), ref colorFixStuffColor, "TD.SettingFixColorStuffDesc".Translate());
			options.CheckboxLabeled("TD.SettingFixUncoloredStuff".Translate(), ref colorFixDominant, "TD.SettingFixUncoloredStuffDesc".Translate());
			options.Gap();

			if (options.ButtonTextLabeled("TD.SettingReapplyColorVariations".Translate(), "TD.Go".Translate()))
				ReapplyAll.Go();
			options.Label("TD.SettingReapplyColorVariationsDesc".Translate());
			options.GapLine();


			//UI / Info
			options.LabelHeader("TD.SettingsHeaderInfo".Translate());
			options.CheckboxLabeled("TD.SettingRememberResourceReadout".Translate(), ref rememberResourceReadout);
			options.CheckboxLabeled("TD.SettingNewGameOpenResourceReadout".Translate(), ref startOpenResourceReadout);
			options.Gap();
			options.CheckboxLabeled("TD.SettingPawnTableHighlightSelected".Translate(), ref pawnTableHighlightSelected, "TD.SettingPawnTableHighlightSelectedDesc".Translate());
			options.CheckboxLabeled("TD.SettingPawnTableArrowMouseover".Translate(), ref pawnTableArrowMouseover);
			options.CheckboxLabeled("TD.SettingPawnTableClickSelect".Translate(), ref pawnTableClickSelect, "TD.SettingPawnTableClickSelectDesc".Translate());
			options.CheckboxLabeled("TD.SettingJumpToSelectedItems".Translate(), ref selectedItemsZoomButton);
			options.Gap();
			options.CheckboxLabeled("TD.SettingTopRightMouseover".Translate(), ref mouseoverInfoTopRight, "TD.SettingTopRightMouseoverDesc".Translate());
			options.CheckboxLabeled("TD.SettingStopForcedSlowdown".Translate(), ref stopForcedSlowdown);
			options.CheckboxLabeled("TD.SettingTradeClose".Translate(), ref changeSpeedAfterTrader);
			options.SliderLabeled("TD.SettingTradeCloseSpeed".Translate(), ref afterTraderSpeed, "{0}x", 0, 4);
			options.Gap();
			options.CheckboxLabeled("TD.SettingResearchingArrow".Translate(), ref researchingArrow);
			options.CheckboxLabeled("TD.SettingSkillGainArrow".Translate(), ref skillUpArrows, "TD.SettingSkillGainArrowDesc".Translate());
			options.CheckboxLabeled("TD.SettingSkillLossArrow".Translate(), ref skillDownArrows, "TD.SettingSkillLossArrowDesc".Translate());
			skillArrows = skillUpArrows || skillDownArrows;
			options.CheckboxLabeled("TD.DropPodWhatDropped".Translate(), ref dropPodWhatDropped);
			options.Gap();
			options.Label("TD.FeatureConditionGreen".Translate());
			options.GapLine();


			//Alerts
			options.LabelHeader("TD.SettingsHeaderAlerts".Translate());
			options.CheckboxLabeled("TD.SettingAlertDeterioration".Translate(), ref alertDeteriorating, "TD.SettingAlertDeteriorationDesc".Translate());
			options.CheckboxLabeled("TD.SettingAlertHeatstroke".Translate(), ref alertHeatstroke);
			options.CheckboxLabeled("TD.SettingAlertBurning".Translate(), ref alertBurning);
			options.CheckboxLabeled("TD.SettingAlertToxic".Translate(), ref alertToxic);
			options.CheckboxLabeled("TD.SettingAlertNoBill".Translate(), ref alertNoBill);
			options.CheckboxLabeled("TD.AlertWindBlocked".Translate(), ref alertWindBlocker);
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
			options.CheckboxLabeled("TD.SettingsZoomToMouse".Translate(), ref zoomToMouse, "TD.SettingsZoomToMouseDesc".Translate());
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
			Scribe_Values.Look(ref billCountEquippedAny, "billCountEquippedAny", true);

			Scribe_Values.Look(ref showOverlayBuildable, "showOverlayBuildable", true);
			Scribe_Values.Look(ref autoOverlayBuildable, "autoOverlayBuildable", true);
			Scribe_Values.Look(ref showOverlayCoverage, "showOverlayCoverage", true);
			Scribe_Values.Look(ref autoOverlayCoverage, "autoOverlayCoverage", true);
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
			Scribe_Values.Look(ref showOverlayPower, "showOverlayPower", true);
			Scribe_Values.Look(ref cheatFertilityUnderGrid, "cheatFertilityUnderGrid", true);
			Scribe_Values.Look(ref overlayOpacity, "overlayOpacity", 1.0f);

			Scribe_Values.Look(ref changeSpeedAfterTrader, "changeSpeedAfterTrader", true);
			Scribe_Values.Look(ref afterTraderSpeed, "afterTraderSpeed", 0);

			Scribe_Values.Look(ref researchingArrow, "researchingArrow", true);
			Scribe_Values.Look(ref skillArrows, "skillArrows", true);
			Scribe_Values.Look(ref skillUpArrows, "skillUpArrows", true);
			Scribe_Values.Look(ref skillDownArrows, "skillDownArrows", true);

			Scribe_Values.Look(ref dropPodWhatDropped, "dropPodWhatDropped", true);

			Scribe_Values.Look(ref showZoneSize, "showZoneSize", true);
			Scribe_Values.Look(ref fieldEdgesRedo, "fieldEdgesRedo", true);
			Scribe_Values.Look(ref zoneHarvestableToggle, "zoneHarvestableToggle", false);
			Scribe_Values.Look(ref areaForTypes, "areaForTypes", true);
			Scribe_Values.Look(ref zoneRefill, "zoneRefill", true);

			Scribe_Values.Look(ref neverHome, "neverHome", true);
			Scribe_Values.Look(ref slaughterZone, "slaughterZone", true);

			Scribe_Values.Look(ref autorebuildDefaultOn, "autorebuildDefaultOn", true);
			Scribe_Values.Look(ref autoRebuildTransportPod, "autoRebuildTransportPod", false);
			Scribe_Values.Look(ref caravanLoadSelection, "caravanLoadSelection", true);
			Scribe_Values.Look(ref caravanSaveManifest, "caravanSaveManifest", true);
			Scribe_Values.Look(ref tradeRequestWorstFirst, "tradeRequestWorstFirst", true);
			Scribe_Values.Look(ref blueprintAnyStuff, "blueprintAnyStuff", true);
			Scribe_Values.Look(ref copyPolicyButton, "copyPolicyButton", false);
			Scribe_Values.Look(ref pawnTableHighlightSelected, "pawnTableHighlightSelected", true);
			Scribe_Values.Look(ref pawnTableArrowMouseover, "pawnTableArrowMouseover", true);
			Scribe_Values.Look(ref rememberResourceReadout, "rememberResourceReadout", true);
			Scribe_Values.Look(ref startOpenResourceReadout, "startOpenResourceReadout", false);
			Scribe_Values.Look(ref pawnTableClickSelect, "pawnTableClickSelect", false);
			Scribe_Values.Look(ref selectedItemsZoomButton, "selectedItemsZoomButton", true);
			Scribe_Values.Look(ref mouseoverInfoTopRight, "mouseoverInfoTopRight", false);
			Scribe_Values.Look(ref stopForcedSlowdown, "stopForcedSlowdown", true);

			Scribe_Values.Look(ref alertDeteriorating, "alertDeteriorating", true);
			Scribe_Values.Look(ref alertHeatstroke, "alertHeatstroke", true);
			Scribe_Values.Look(ref alertBurning, "alertBurning", true);
			Scribe_Values.Look(ref alertToxic, "alertToxic", true);
			Scribe_Values.Look(ref alertNoBill, "alertNoBill", true);
			Scribe_Values.Look(ref alertWindBlocker, "alertWindBlocker", true);

			Scribe_Values.Look(ref areasUnlimited, "areasUnlimited", true);
			Scribe_Values.Look(ref matchGrowButton, "matchGrowButton", true);
			Scribe_Values.Look(ref deepDrillRandom, "deepDrillRandom", true);

			Scribe_Values.Look(ref zoomToMouse, "zoomToMouse", false);

			Scribe_Values.Look(ref showToggleLearning, "showToggleLearning", true);
			Scribe_Values.Look(ref showToggleZone, "showToggleZone", true);
			Scribe_Values.Look(ref showToggleBeauty, "showToggleBeauty", true);
			Scribe_Values.Look(ref showToggleRoomstats, "showToggleRoomstats", true);
			Scribe_Values.Look(ref showToggleColonists, "showToggleColonists", true);
			Scribe_Values.Look(ref showToggleRoof, "showToggleRoof", true);
			Scribe_Values.Look(ref showToggleHomeArea, "showToggleHomeArea", true);
			Scribe_Values.Look(ref showToggleRebuild, "showToggleRebuild", true);
			Scribe_Values.Look(ref showToggleCategorized, "showToggleCategorized", true);

			Scribe_Values.Look(ref colorVariation, "colorVariation", false);
			Scribe_Values.Look(ref colorGenerator, "colorGenerator", false);
			Scribe_Values.Look(ref colorGenChance, "colorGenChance", 0.5f);
			Scribe_Values.Look(ref colorFixStuffColor, "colorFixStuffColor", false);
			Scribe_Values.Look(ref colorFixDominant, "colorFixDominant", false);
			Scribe_Values.Look(ref colorRedoWarned, "colorRedoWarned", false);
		}
	}
}