using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using HarmonyLib;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(DeepDrillUtility), "GetBaseResource")]
	class DeepDrillRandomRock
	{
		//public static ThingDef GetBaseResource(Map map)
		public static bool Prefix(ref ThingDef __result, Map map)
		{
			if (!Settings.Get().deepDrillRandom) return true;

			if (!map.Biome.hasBedrock)
			{
				__result = null;
				return false;
			}
			__result = (from rock in Find.World.NaturalRockTypesIn(map.Tile)
									select rock.building.mineableThing).RandomElementWithFallback<ThingDef>();
			return false;
		}
	}

	public struct DrillData : IExposable
	{
		public ThingDef resDef;
		public int countPresent;
		public IntVec3 cell;
		public bool result;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref resDef, "resourceDef");
			Scribe_Values.Look(ref countPresent, "count");
			Scribe_Values.Look(ref cell, "pos");
			Scribe_Values.Look(ref result, "result");
		}
	}
	public class NextDrillResourceComp : MapComponent
	{
		public Dictionary<IntVec3, DrillData> nextResources;

		public NextDrillResourceComp(Map map) : base(map)
		{
			nextResources = new Dictionary<IntVec3, DrillData>();
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref nextResources, "nextResources");
		}

		public static Dictionary<IntVec3, DrillData> Get(Map map)
		{
			return map.GetComponent<NextDrillResourceComp>().nextResources;
		}

	}

	[StaticConstructorOnStartup]
	//[HarmonyPatch(typeof(DeepDrillUtility), "GetNextResource", new Type[] 
	//{ typeof(IntVec3), typeof(Map), typeof(ThingDef).MakeByRefType(), typeof(int).MakeByRefType(), typeof(IntVec3).MakeByRefType()})]
	public static class DeepDrillDatabase
	{
		static DeepDrillDatabase()
		{
			//harmony annotation can't seem to handle ambiguous name with ref params
			MethodInfo topatch = AccessTools.Method(typeof(DeepDrillUtility), "GetNextResource", new Type[]
			{ typeof(IntVec3), typeof(Map), typeof(ThingDef).MakeByRefType(), typeof(int).MakeByRefType(), typeof(IntVec3).MakeByRefType() } );

			HarmonyInstance.Create("Uuugggg.rimworld.TD_Enhancement_Pack.main").Patch(topatch,
				new HarmonyMethod(typeof(DeepDrillDatabase), "Prefix"), new HarmonyMethod(typeof(DeepDrillDatabase), "Postfix"));
		}

		//public static bool GetNextResource(IntVec3 p, Map map, out ThingDef resDef, out int countPresent, out IntVec3 cell)
		public static bool Prefix(ref bool __result, IntVec3 p, Map map, ref ThingDef resDef, ref int countPresent, ref IntVec3 cell)
		{
			if(NextDrillResourceComp.Get(map).TryGetValue(p, out DrillData nextDef))
			{
				resDef = nextDef.resDef;
				countPresent = nextDef.countPresent;
				cell = nextDef.cell;
				__result = nextDef.result;
				return false;
			}
			return true;
		}

		public static void Postfix(bool __result, IntVec3 p, Map map, ref ThingDef resDef, ref int countPresent, ref IntVec3 cell)
		{
			var nextResources = NextDrillResourceComp.Get(map);
			if (nextResources.ContainsKey(p)) return;

			//Use NextDrillResourceComp if it has it, even if settings is off
			if (!Settings.Get().deepDrillRandom) return;

			Log.Message($"Adding drill data {p}, {resDef}, {countPresent}, {cell}, {__result}");
			DrillData drillData;
			drillData.resDef = resDef;
			drillData.countPresent = countPresent;
			drillData.cell = cell;
			drillData.result = __result;
			nextResources[p] = drillData;
		}
	}

	[HarmonyPatch(typeof(CompDeepDrill), "TryProducePortion")]
	public static class ClearDDDB
	{
		//private void TryProducePortion(float yieldPct)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo GetNextResourceInfo = AccessTools.Method(typeof(CompDeepDrill), "GetNextResource");

			MethodInfo ClearInfo = AccessTools.Method(typeof(ClearDDDB), nameof(Clear));

			foreach(CodeInstruction i in instructions)
			{
				yield return i;
				if(i.opcode == OpCodes.Call && i.operand.Equals(GetNextResourceInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);//this (CompDeepDrill)
					yield return new CodeInstruction(OpCodes.Call, ClearInfo);//this.Clear()
				}
			}
		}
		public static void Clear(CompDeepDrill ddComp)
		{
			Log.Message($"Removing Deep Drill {ddComp.parent.Position}");
			NextDrillResourceComp.Get(ddComp.parent.Map).Remove(ddComp.parent.Position);
		}
	}

}
