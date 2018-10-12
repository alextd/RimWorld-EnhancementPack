using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Harmony;

namespace TD.Utilities
{
	public static class Listing_StandardExtensions
	{
		public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, string format, float min = 0f, float max = 100f, string tooltip = null)
		{
			float fVal = val;
			ls.SliderLabeled(label, ref fVal, format, min, max);
			val = (int)fVal;
		}
		public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, string format, float min = 0f, float max = 1f, string tooltip = null)
		{
			Rect rect = ls.GetRect(Text.LineHeight);
			Rect rect2 = rect.LeftPart(.70f).Rounded();
			Rect rect3 = rect.RightPart(.30f).Rounded().LeftPart(.67f).Rounded();
			Rect rect4 = rect.RightPart(.10f).Rounded();

			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect2, label);

			float result = Widgets.HorizontalSlider(rect3, val, min, max, true);
			val = result;
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect4, String.Format(format, val));
			if (!tooltip.NullOrEmpty())
			{
				TooltipHandler.TipRegion(rect, tooltip);
			}

			Text.Anchor = anchor;
			ls.Gap(ls.verticalSpacing);
		}

		public static void BeginScrollViewEx(this Listing_Standard listing, Rect rect, ref Vector2 scrollPosition, ref Rect viewRect)
		{
			//Widgets.BeginScrollView(rect, ref scrollPosition, viewRect, true);
			//rect.height = 100000f;
			//rect.width -= 20f;
			//this.Begin(rect.AtZero());

			//Need BeginGroup before ScrollView, listingRect needs rect.width-=20 but the group doesn't

			GUI.BeginGroup(rect);
			Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect, true);
			
			rect.height = 100000f;
			rect.width -= 20f;
			//base.Begin(rect.AtZero());


			//listing.listingRect = rect;
			AccessTools.Field(typeof(Listing_Standard), "listingRect").SetValue(listing, rect);
			//listing.columnWidthInt = listing.listingRect.width;
			AccessTools.Field(typeof(Listing_Standard), "columnWidthInt").SetValue(listing, rect.width);
			//listing.curX = 0f;
			AccessTools.Field(typeof(Listing_Standard), "curX").SetValue(listing, 0);
			//listing.curY = 0f;
			AccessTools.Field(typeof(Listing_Standard), "curY").SetValue(listing, 0);

			Text.Font = (GameFont)AccessTools.Field(typeof(Listing_Standard), "font").GetValue(listing);
		}
	}
}