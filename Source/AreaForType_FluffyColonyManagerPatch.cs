using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using Harmony;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	public static class PatchFluffy
	{
		public static string[] fluffyTypesColonist = {
"ManagerTab_Foraging",
"ManagerTab_Forestry",
"ManagerTab_Hunting",
"ManagerTab_Mining",
		};
		public static string[] fluffyTypesAnimal = {
"ManagerTab_Livestock",
		};
		public static string[] fluffyTypesNeither = {
"ManagerTab_ImportExport",
"ManagerTab_Power",
"ManagerTab_Production",
"ManagerTab_Trading",
"ManagerTab_Overview"
		};
		static PatchFluffy()
		{
			HarmonyInstance harmony = HarmonyInstance.Create("Uuugggg.rimworld.TD_Enhancement_Pack.fluffy_patches");

			//Patch the call to AssignableAsAllowed to check area by colonist/animal
			MethodInfo FluffyMethodInfo = AccessTools.Method("AreaAllowedGUI:DoAllowedAreaSelectors",
				new Type[] { typeof(Rect), typeof(Area).MakeByRefType(), typeof(Map), typeof(float) });
			if (FluffyMethodInfo != null)
				harmony.Patch(FluffyMethodInfo,
				transpiler: new HarmonyMethod(typeof(PatchFluffy), "Transpiler"));


			//Colonist tabs:
			HarmonyMethod ColonistTabOpenPrefix = new HarmonyMethod(typeof(PatchFluffy), nameof(PreFixOpenForColonists));
			foreach (string typeName in fluffyTypesColonist)
				if (AccessTools.Method(typeName + ":PreOpen") is MethodInfo info)
					harmony.Patch(info, ColonistTabOpenPrefix);

			//Animal tabs:
			HarmonyMethod AnimalTabOpenPrefix = new HarmonyMethod(typeof(PatchFluffy), nameof(PreFixOpenForAnimals));
			foreach (string typeName in fluffyTypesAnimal)
				if (AccessTools.Method(typeName + ":PreOpen") is MethodInfo info)
					harmony.Patch(info, AnimalTabOpenPrefix);

			//Other tabs:
			HarmonyMethod OtherTabOpenPrefix = new HarmonyMethod(typeof(PatchFluffy), nameof(PreFixOpenForNeither));
			foreach (string typeName in fluffyTypesNeither)
				if (AccessTools.Method(typeName + ":PreOpen") is MethodInfo info)
					harmony.Patch(info, OtherTabOpenPrefix);

		}

		//public static void DoAllowedAreaSelectors(Rect rect,
		//																							 ref Area area,
		//																							 Map map,
		//																							 float lrMargin = 0)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return Harmony.Transpilers.MethodReplacer(instructions,
				AccessTools.Method(typeof(Area), nameof(Area.AssignableAsAllowed)),
				AccessTools.Method(typeof(PatchFluffy), nameof(AssignableAsAllowedWithContext)));
		}

		public static bool drawingForAnimals;
		public static bool drawingForColonists;
		public static bool AssignableAsAllowedWithContext(Area area)
		{
			if (!area.AssignableAsAllowed()) return false;

			if (!Settings.Get().areaForTypes) return true;

			var comp = area.Map.GetComponent<MapComponent_AreaOrder>();
			if (drawingForColonists)
				return !comp.notForColonists.Contains(area);
			else if(drawingForAnimals)
				return !comp.notForAnimals.Contains(area);

			return true;
		}

		public static void PreFixOpenForColonists()
		{
			drawingForColonists = true;
			drawingForAnimals = false;
		}

		public static void PreFixOpenForAnimals()
		{
			drawingForColonists = false;
			drawingForAnimals = true;
		}

		public static void PreFixOpenForNeither()
		{
			drawingForColonists = false;
			drawingForAnimals = false;
		}
	}
}
