using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Dialog_ManageOutfits), nameof(Dialog_ManageOutfits.DoWindowContents))]
	class CopyOutfit
	{
		//public override void DoWindowContents(Rect inRect);
		public static void Postfix(Rect inRect, ref Outfit ___selOutfitInt)
		{
			if (___selOutfitInt == null) return;

			Rect butRect = new Rect(inRect.width - 150f, 0f, 150f, 35f);//otherwise tediously transpile it in, this dialog isn't using Listing
			if (Widgets.ButtonText(butRect, "Copy Outfit"))
			{
				ThingFilter selFilter = ___selOutfitInt.filter;
				___selOutfitInt = Current.Game.outfitDatabase.MakeNewOutfit();
				___selOutfitInt.filter.CopyAllowancesFrom(selFilter);
			}
		}
	}
}
