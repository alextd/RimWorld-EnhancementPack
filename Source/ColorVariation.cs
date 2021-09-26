﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using TD.Utilities;

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	static class ColorVariation
	{
		static ColorVariation()
		{
			Harmony harmony = new Harmony("Uuugggg.rimworld.TD_Enhancement_Pack.main");

			//Want to patch Toils_Recipe.FinishRecipeAndStartStoringProduct, it sets initAction = delegate
			harmony.PatchGeneratedMethod(typeof(Toils_Recipe),
				m => m.Name.Contains("FinishRecipeAndStartStoringProduct"),
				transpiler: new HarmonyMethod(typeof(ColorVariation), nameof(Toils_Recipe_Transpiler)));

			//Want to patch GenRecipe.MakeRecipeProducts, it's a yield return method
			harmony.PatchGeneratedMethod(typeof(GenRecipe),
				m => m.Name.Contains("MakeRecipeProducts") ||
				(m.DeclaringType.Name.Contains("MakeRecipeProducts") && m.Name.Contains("MoveNext")),
				transpiler: new HarmonyMethod(typeof(ColorVariation), nameof(GenRecipe_Transpiler)));
		}

		//[HarmonyPatch(typeof(Toils_Recipe), nameof(Toils_Recipe.FinishRecipeAndStartStoringProduct))]
		//actually patching compiler generated method for 'initAction =' delegate
		public static IEnumerable<CodeInstruction> Toils_Recipe_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo MakeRecipeProductsInfo = AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts));

			foreach (CodeInstruction i in instructions)
			{
				yield return i;

				if (i.Calls(MakeRecipeProductsInfo))
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ColorVariation), nameof(Variate)));
				}
			}
		}

		public static IEnumerable<Thing> Variate(IEnumerable<Thing> things)
		{
			foreach (Thing t in things)
			{
				VaryColor(t);
				yield return t;
			}
		}

		public static void VaryColor(Thing t)
		{
			if (!Settings.Get().colorGenerator && !Settings.Get().colorVariation) return;

			if (t is ThingWithComps thing)
			{
				if (thing.GetComp<CompColorable>() is CompColorable comp)
				{
					Log.Message($"{thing} color was {thing.DrawColor}/{comp.Active}:{comp.Color}");
					Color color = thing.DrawColor;

					//Override with Color generator?
					//This is sorta redundant since the recipe just overwrote what might've been a color generated color, 
					//but only cloth allows it, but since it's overwritten it doesn't matter that cloth allows it. Aeh.
					bool generatedColor = Settings.Get().colorGenerator
						&& thing.def.colorGenerator != null
						&& (thing.Stuff == null || thing.Stuff.stuffProps.allowColorGenerators)
						&& Rand.Value < Settings.Get().colorGenChance;//.2 = 20% chance to colorgenerate instead of stuff color

					if (generatedColor) 
					{
						color = thing.def.colorGenerator.NewRandomizedColor();
						Log.Message($"using generated {color}");
					}

					//Variation
					if (Settings.Get().colorVariation &&
						(!Settings.Get().colorGenNoVariation || !generatedColor))
					{
						Log.Message($"Varying the color...");
						//Deviate a little.
						Color.RGBToHSV(color, out float h, out float s, out float v);
						//hsv makes colored things look more varied, and whiter things less varied.
						color = Color.HSVToRGB(
						Mathf.Clamp01(h + (Rand.Value - 0.5f) / 10),	// +/- 5%
						Mathf.Clamp01(s + (Rand.Value - 0.5f) / 10),
						Mathf.Clamp01(v + (Rand.Value - 0.5f) / 10));
					}

					if (color != thing.DrawColor)
						comp.SetColor(color);
					Log.Message($"{thing} color now {thing.DrawColor}/{comp.Active}:{comp.Color}");
				}
			}
		}

		//[HarmonyPatch(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts))]
		//public static IEnumerable<Thing> MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
		//actually patching compiler generated method due to yield return business
		public static IEnumerable<CodeInstruction> GenRecipe_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo GetDrawColorInfo = AccessTools.Property(typeof(Thing), nameof(Thing.DrawColor)).GetGetMethod();

			foreach (CodeInstruction i in instructions)
			{
				if (i.Calls(GetDrawColorInfo))
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ColorVariation), nameof(StuffColor)));
				}
				else
					yield return i;

			}
		}

		//public virtual Color DrawColor
		public static Color StuffColor(Thing stuff)
		{
			if (!Settings.Get().colorFixStuffColor) return stuff.DrawColor;

			if (stuff.TryGetComp<CompColorable>() is CompColorable comp && comp.Active)
			{
				Log.Message($"Properly using {stuff}'s CompColorable: {comp.Color}");
				return comp.Color;
			}

			Color result = stuff.def.stuffProps?.color ?? stuff.DrawColor;
			Log.Message($"Properly using {stuff}'s color: {result}");
			return result;
		}
	}

	[HarmonyPatch(typeof(Toils_Recipe), "CalculateDominantIngredient")]
	public static class Dominate
	{
		//private static Thing CalculateDominantIngredient(Job job, List<Thing> ingredients)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return Transpilers.MethodReplacer(instructions,
				AccessTools.Method(typeof(GenCollection), "RandomElementByWeight").MakeGenericMethod(typeof(Thing)),
				AccessTools.Method(typeof(Dominate), nameof(StuffOnly)));
		}

		//public static T RandomElementByWeight<T>(this IEnumerable<T> source, Func<T, float> weightSelector)
		public static Thing StuffOnly(this IEnumerable<Thing> source, Func<Thing, float> weightSelector)
		{
			if (!Settings.Get().colorFixDominant) return source.RandomElementByWeight(weightSelector);

			List<Thing> stuffThings = source.Where(t => t.def.IsStuff).ToList();
			Log.Message($"Removed ({source.Where(t => !t.def.IsStuff).ToList().ToStringSafeEnumerable()}) from list of ingredients with color");
			if (stuffThings.NullOrEmpty())
				return source.RandomElementByWeight(weightSelector);
			return stuffThings.RandomElementByWeight(weightSelector);
			//return source.MaxBy(weightSelector);	//only the one ever
		}
	}

	public static class ReapplyAll
	{
		public static void ReDo(IEnumerable<Thing> things)
		{
			//Re-do recipe color making
			foreach(Thing thing in things.Where(t => t.TryGetComp<CompColorable>() != null && (!t.def.costList.NullOrEmpty() || t.def.costStuffCount > 0)))
			{
				if (thing is Apparel)//Vanilla only has CompColorable on apparel but mods might make other non-craftable things colorable.
				{
					Log.Message($"REDOING {thing}");
					if (thing.def.MadeFromStuff)
						thing.SetColor(thing.Stuff.stuffProps.color);
					else // This will not know if the original stuff had compcolorable set, just ignores it, oh well.
						thing.SetColor(thing.def.costList?.Where(c => c.thingDef.IsStuff).RandomElementByWeightWithFallback(c => c.count)?.thingDef.stuffProps.color ?? thing.DrawColor);
					ColorVariation.VaryColor(thing);
				}
			}
			//Variation
		}

		public static MethodInfo ApparelChangedInfo = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker. Notify_ApparelChanged));
		public static void ApparelChanged(this Pawn_ApparelTracker appTracker) =>
			ApparelChangedInfo.Invoke(appTracker, new object[] { });
		public static void Go()
		{
			if(!Settings.Get().colorRedoWarned)
			{
				Find.WindowStack.Add(new Dialog_MessageBox("TD.WarningReColorAll".Translate(), "TD.OKIGetIt".Translate(), title: "TD.HoldOn".Translate()));
				Settings.Get().colorRedoWarned = true;
				return;
			}
			Log.Message("GOING");
			foreach(Map map in Find.Maps)
			{
				foreach(Pawn pawn in map.mapPawns.FreeColonists)
				{
					Log.Message($"MR {pawn}");
					ReDo(pawn.inventory.GetDirectlyHeldThings());
					ReDo(pawn.apparel.WornApparel.Cast<Thing>());
					//pawn.apparel.Notify_ApparelAdded(null);
					pawn.apparel.ApparelChanged();
				}

				ReDo(map.listerThings.AllThings);
			}
		}
	}
}
