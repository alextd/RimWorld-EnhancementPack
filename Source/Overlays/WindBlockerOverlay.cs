using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Harmony;

namespace TD_Enhancement_Pack
{
	[StaticConstructorOnStartup]
	class WindBlockerOverlay : BaseOverlay
	{
		public WindBlockerOverlay(Map m) : base(m) { }

		public override bool ShowCell(int index)
		{
			return map.roofGrid.Roofed(index) || 
				map.thingGrid.ThingsListAtFast(index).Any(t => t.def.blockWind);
		}

		public override Color GetCellExtraColor(int index) => Color.blue;


		public override bool ShouldAutoDraw() => Settings.Get().autoOverlayWindBlocker;
		public override Type AutoDesignator() => typeof(Designator_Build);
		public override bool DesignatorVerifier(Designator des)
		{
			return des is Designator_Build desBuild &&
				desBuild.PlacingDef is ThingDef def &&
				def.HasComp(typeof(CompPowerPlantWind));
		}
	}

	[HarmonyPatch(typeof(RoofGrid), "SetRoof")]
	static class SetRoofSetDirty
	{
		public static void Postfix(Map ___map)
		{
			BaseOverlay.SetDirty(typeof(WindBlockerOverlay), ___map);
		}
	}

	[HarmonyPatch(typeof(ThingGrid), "Register")]
	public static class ThingDirtierRegister_WindBlocker
	{
		public static void Postfix(Map ___map)
		{
			BaseOverlay.SetDirty(typeof(WindBlockerOverlay), ___map);
		}
	}

	[HarmonyPatch(typeof(ThingGrid), "Deregister")]
	public static class ThingDirtierDeregister_WindBlocker
	{
		public static void Postfix(Map ___map)
		{
			BaseOverlay.SetDirty(typeof(WindBlockerOverlay), ___map);
		}
	}
}
