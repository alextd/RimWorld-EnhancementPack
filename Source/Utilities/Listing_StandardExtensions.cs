using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using HarmonyLib;

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

		//listing.listingRect = rect;
		public static FieldInfo rectInfo = AccessTools.Field(typeof(Listing_Standard), "listingRect");
		//listing.columnWidthInt = listing.listingRect.width;
		public static FieldInfo widthInfo = AccessTools.Field(typeof(Listing_Standard), "columnWidthInt");
		//listing.curX = 0f;
		public static FieldInfo curXInfo = AccessTools.Field(typeof(Listing_Standard), "curX");
		//listing.curY = 0f;
		public static FieldInfo curYInfo = AccessTools.Field(typeof(Listing_Standard), "curY");
		public static FieldInfo fontInfo = AccessTools.Field(typeof(Listing_Standard), "font");
		public static void BeginScrollViewEx(this Listing_Standard listing, Rect rect, ref Vector2 scrollPosition, Rect viewRect)
		{
			//Widgets.BeginScrollView(rect, ref scrollPosition, viewRect, true);
			//rect.height = 100000f;
			//rect.width -= 20f;
			//this.Begin(rect.AtZero());

			//Need BeginGroup before ScrollView, listingRect needs rect.width-=20 but the group doesn't

			Widgets.BeginGroup(rect);
			Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect, true);
			
			rect.height = 100000f;
			rect.width -= 20f;
			//base.Begin(rect.AtZero());


			//listing.listingRect = rect;
			rectInfo.SetValue(listing, rect);
			//listing.columnWidthInt = listing.listingRect.width;
			widthInfo.SetValue(listing, rect.width);
			//listing.curX = 0f;
			curXInfo.SetValue(listing, 0);
			//listing.curY = 0f;
			curYInfo.SetValue(listing, 0);

			Text.Font = (GameFont)fontInfo.GetValue(listing);
		}

		public static void LabelHeader(this Listing_Standard listing, string label, float maxHeight = -1, string tooltip = null)
		{
			Text.Font = GameFont.Medium;
			listing.Label(label, maxHeight, tooltip);
			Text.Font = GameFont.Small;
		}

		//1.3 just removed Listing Scrollviews?
		public static void EndScrollView(this Listing_Standard listing, ref Rect viewRect)
		{
			viewRect = new Rect(0f, 0f, listing.ColumnWidth, listing.CurHeight);
			Widgets.EndScrollView();
			listing.End();
		}
	}
}