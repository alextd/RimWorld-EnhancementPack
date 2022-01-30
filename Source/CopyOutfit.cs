using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using HarmonyLib;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Dialog_ManageOutfits), nameof(Dialog_ManageOutfits.DoWindowContents))]
	class CopyOutfit
	{
		//public override void DoWindowContents(Rect inRect);
		public static void Postfix(Rect inRect, ref Outfit ___selOutfitInt)
		{
			if (!Mod.settings.copyPolicyButton) return;
			if (___selOutfitInt == null) return;

			Rect butRect = new Rect(inRect.width - 150f, 0f, 150f, 35f);//otherwise tediously transpile it in, this dialog isn't using Listing
			if (Widgets.ButtonText(butRect, "TD.MakeCopy".Translate()))
			{
				ThingFilter selFilter = ___selOutfitInt.filter;
				___selOutfitInt = Current.Game.outfitDatabase.MakeNewOutfit();
				___selOutfitInt.filter.CopyAllowancesFrom(selFilter);
			}
		}
	}

	//While I'm here, I might as well copy Food Restriction. Drug Policies don't have easily copieable settings

	[HarmonyPatch(typeof(Dialog_ManageFoodRestrictions), nameof(Dialog_ManageFoodRestrictions.DoWindowContents))]
	class CopyFoodRestriction
	{
		//public override void DoWindowContents(Rect inRect);
		public static void Postfix(Rect inRect, ref FoodRestriction ___selFoodRestrictionInt)
		{
			if (!Mod.settings.copyPolicyButton) return;
			if (___selFoodRestrictionInt == null) return;

			Rect butRect = new Rect(inRect.width - 150f, 0f, 150f, 35f);//otherwise tediously transpile it in, this dialog isn't using Listing
			if (Widgets.ButtonText(butRect, "TD.MakeCopy".Translate()))
			{
				ThingFilter selFilter = ___selFoodRestrictionInt.filter;
				___selFoodRestrictionInt = Current.Game.foodRestrictionDatabase.MakeNewFoodRestriction();
				___selFoodRestrictionInt.filter.CopyAllowancesFrom(selFilter);
			}
		}
	}
}
