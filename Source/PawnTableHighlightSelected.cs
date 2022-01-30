using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(PawnColumnWorker_Label), "DoCell")]
	class LabelAddSelection
	{
		//public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		public static bool dragSelect = false;
		public static bool dragDeselect = false;
		public static bool dragJump = false;
		public static void Prefix(Rect rect, Pawn pawn, PawnTable table)
		{
			if (!Settings.settings.pawnTableClickSelect) return;

			//from DoCell:
			Rect rowRect = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));

			//Ripped out of My List Everything mod
			if (Mouse.IsOver(rowRect))
			{
				if (Event.current.type == EventType.MouseDown)
				{
					if (Event.current.clickCount == 2 && Event.current.button == 0)
					{
						PawnTableAddSelection.selectAllDef = pawn.def;
					}
					if (Event.current.shift)
					{
						if (Find.Selector.IsSelected(pawn))
						{
							dragDeselect = true;
							Find.Selector.Deselect(pawn);
						}
						else
						{
							dragSelect = true;
							Find.Selector.Select(pawn);
						}
					}
					else if (Event.current.alt)
					{
						Find.MainTabsRoot.EscapeCurrentTab(false);
						CameraJumper.TryJumpAndSelect(pawn);
					}
					else
					{
						if (Event.current.button == 1)
						{
							CameraJumper.TryJump(pawn);
							dragJump = true;
						}
						else if (Find.Selector.IsSelected(pawn))
						{
							CameraJumper.TryJump(pawn);
							dragSelect = true;
						}
						else
						{
							Find.Selector.ClearSelection();
							Find.Selector.Select(pawn);
							dragSelect = true;
						}
					}
				}
				if (Event.current.type == EventType.MouseDrag)
				{
					if (dragJump)
						CameraJumper.TryJump(pawn);
					else if (dragSelect)
						Find.Selector.Select(pawn, false);
					else if (dragDeselect)
						Find.Selector.Deselect(pawn);
				}
			}
		}

		//Don't do normal selecion button
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return Transpilers.MethodReplacer(
				Transpilers.MethodReplacer(instructions,
				AccessTools.Method(typeof(Widgets), nameof(Widgets.ButtonInvisible)),
				AccessTools.Method(typeof(LabelAddSelection), nameof(NoButtonInvisible))),
				AccessTools.Method(typeof(TooltipHandler), nameof(TooltipHandler.TipRegion), new Type[] { typeof(Rect), typeof(TipSignal)}),
				AccessTools.Method(typeof(LabelAddSelection), nameof(NoTipRegion)));
			;
		}

		//public static bool ButtonInvisible(Rect butRect, bool doMouseoverSound = false)
		public static bool NoButtonInvisible(Rect butRect, bool doMouseoverSound)
		{
			if (!Settings.settings.pawnTableClickSelect) return Widgets.ButtonInvisible(butRect, doMouseoverSound);
			return false;
		}

		//public static void TipRegion(Rect rect, TipSignal tip)
		public static void NoTipRegion(Rect rect, TipSignal tip)
		{
			if (!Settings.settings.pawnTableClickSelect)
				TooltipHandler.TipRegion(rect, tip);
		}

		//Draw selection and mouseover highlights
		public static void Postfix(Rect rect, Pawn pawn)
		{
			//from DoCell:
			Rect rowRect = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));

			if (Settings.settings.pawnTableHighlightSelected)
				if (Find.Selector.IsSelected(pawn))
					Widgets.DrawHighlightSelected(rowRect);

			if (Settings.settings.pawnTableArrowMouseover)
				if (Mouse.IsOver(rowRect))
				{
					Vector3 center = UI.UIToMapPosition((float)(UI.screenWidth / 2), (float)(UI.screenHeight / 2));
					bool arrow = (center - pawn.DrawPos).MagnitudeHorizontalSquared() >= 121f;//Normal arrow is 9^2, using 11^1 seems good too.
					TargetHighlighter.Highlight(pawn, arrow, true, true);
				}
		}
	}

	[HarmonyPatch(typeof(PawnTable), nameof(PawnTable.PawnTableOnGUI))]
	public static class PawnTableAddSelection
	{
		public static ThingDef selectAllDef;

		//public void PawnTableOnGUI(Vector2 position);
		public static void Prefix()
		{
			if (!Settings.settings.pawnTableClickSelect) return;

			//Clear dragging status before table draws
			if (!Input.GetMouseButton(0))
			{
				LabelAddSelection.dragSelect = false;
				LabelAddSelection.dragDeselect = false;
			}
			if (!Input.GetMouseButton(1))
				LabelAddSelection.dragJump = false;

			selectAllDef = null;
		}

		public static void Postfix(List<Pawn> ___cachedPawns)
		{
			if (!Settings.settings.pawnTableClickSelect) return;

			//Select all for double-click
			if (selectAllDef != null)
				foreach (Pawn pawn in ___cachedPawns)
					if (pawn.def == selectAllDef)
						Find.Selector.Select(pawn, false);
		}
	}
}
