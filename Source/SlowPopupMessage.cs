using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(GenSpawn), "Spawn", new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(bool) })]
	class SlowPopupMessage
	{
		//public static Thing Spawn(Thing newThing, IntVec3 loc, Map map, Rot4 rot, bool respawningAfterLoad = false)
		public static void Prefix(Thing newThing)
		{
			if (!Settings.Get().moteTextRealtime) return;

			if (newThing is MoteText mote)
				mote.def.mote.realTime = true;
		}
	}
}
