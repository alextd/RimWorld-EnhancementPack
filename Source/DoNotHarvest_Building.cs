using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using Harmony;
using TD.Utilities;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Building_PlantGrower), "GetGizmos")]
	class DoNotHarvest_Building_Gizmo
	{
		//public override IEnumerable<Gizmo> GetGizmos()
		static void Postfix(Building_PlantGrower __instance, ref IEnumerable<Gizmo> __result)
		{
			if (!Settings.Get().zoneHarvestableToggle) return;

			List<Gizmo> result = new List<Gizmo>(__result);

			Gizmo harvestGizmo = new Command_Toggle
			{
				defaultLabel = "TD.AllowHarvesting".Translate(),
				defaultDesc = "TD.AllowHarvestingDesc".Translate(),
				icon = ContentFinder<UnityEngine.Texture2D>.Get("UI/Designators/Harvest", true),
				isActive = (() => __instance.CanHarvest()),
				toggleAction = delegate
				{
					__instance.ToggleHarvest();
				}
			};
			result.Add(harvestGizmo);

			// make caller use the list
			__result = result.AsEnumerable();
		}
	}

	[StaticConstructorOnStartup]
	static class DoNotHarvest_Building
	{
		//WorkGiver_Grower has one PotentialWorkCellsGlobal for both subclasses
		//public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)Thing t, HashSet<Thing> nearbyNeeders, IConstructible constructible, Pawn pawn)

		static DoNotHarvest_Building()
		{
			HarmonyMethod transpiler = new HarmonyMethod(typeof(DoNotHarvest_Building), nameof(DoNotHarvest_Building.Transpiler));
			HarmonyInstance harmony = HarmonyInstance.Create("Uuugggg.rimworld.TD_Enhancement_Pack.main");

			MethodInfo IsForbiddenInfo = AccessTools.Method(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(Thing), typeof(Pawn)});
			Func<MethodInfo, bool> check = delegate (MethodInfo method)
			{
				DynamicMethod dm = DynamicTools.CreateDynamicMethod(method, "-unused");

				return (Harmony.ILCopying.MethodBodyReader.GetInstructions(dm.GetILGenerator(), method).
					Any(ilcode => ilcode.operand == IsForbiddenInfo));
			};

			harmony.PatchGeneratedMethod(typeof(WorkGiver_Grower), check, transpiler: transpiler);
		}
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
		{
			//Replace
			MethodInfo IsForbiddenInfo = AccessTools.Method(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(Thing), typeof(Pawn) });

			//With
			MethodInfo IsForbiddenByTypeInfo = AccessTools.Method(typeof(DoNotHarvest_Building), "IsForbiddenByType");

			//Call with WorkGiver_Grower, which is this.$this since it's compiler generated iterator

			//.field assembly class RimWorld.WorkGiver_Grower $this
			
			FieldInfo ThisThis = AccessTools.GetDeclaredFields(method.DeclaringType).First(f => f.FieldType == typeof(WorkGiver_Grower));

			foreach (var i in instructions)
			{
				if (i.operand == IsForbiddenInfo)
				{
					i.operand = IsForbiddenByTypeInfo;
					yield return new CodeInstruction(OpCodes.Ldarg_0);//this
					yield return new CodeInstruction(OpCodes.Ldfld, ThisThis);//this.$this
				}
				yield return i;
			}
		}

		//public static bool IsForbidden(this Thing t, Pawn pawn)
		public static bool IsForbiddenByType(Thing thing, Pawn pawn, WorkGiver_Grower workGiver)
		{
			if (!Settings.Get().zoneHarvestableToggle
				|| !(workGiver is WorkGiver_GrowerHarvest))
				return thing.IsForbidden(pawn);

			//WorkGiver_GrowerHarvest now
			//Reimplementing IsForbidden with ForbidHarvestBuildingMapComp instead of forbidden comp
			if (!ForbidUtility.CaresAboutForbidden(pawn, false))
			{
				return false;
			}
			if (thing.Spawned && thing.Position.IsForbidden(pawn))
			{
				return true;
			}
			//if (thing.IsForbidden(pawn.Faction) || thing.IsForbidden(pawn.HostFaction))
			//{
			//	return true;
			//}
			return !thing.CanHarvest();
		}
	}

	public class ForbidHarvestBuildingMapComp : MapComponent
	{
		public List<Building_PlantGrower> harvestForbidden;
		public ForbidHarvestBuildingMapComp(Map map) : base(map)
		{
			harvestForbidden = new List<Building_PlantGrower>();
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref harvestForbidden, "harvestForbidden", LookMode.Reference);
		}
	}

	public static class Building_PlantGrower_Extensions
	{
		public static bool CanHarvest(this Thing thing)
		{
			return thing is Building_PlantGrower building &&
				!building.Map.GetComponent<ForbidHarvestBuildingMapComp>().harvestForbidden.Contains(building);
		}
		public static void ToggleHarvest(this Thing thing)
		{
			if (thing is Building_PlantGrower building)
			{
				if (building.Map.GetComponent<ForbidHarvestBuildingMapComp>().harvestForbidden.Contains(building))
					building.Map.GetComponent<ForbidHarvestBuildingMapComp>().harvestForbidden.Remove(building);
				else
					building.Map.GetComponent<ForbidHarvestBuildingMapComp>().harvestForbidden.Add(building);
			}
		}
	}
}
