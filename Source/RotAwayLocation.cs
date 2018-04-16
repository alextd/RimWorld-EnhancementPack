using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(CompRottable), "Tick")]
	public static class RotAwayLocation
	{
		//CompRottable  private void Tick(int interval)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo MessageNoLocInfo = AccessTools.Method(typeof(Messages), "Message", new Type[] { typeof(string), typeof(MessageTypeDef)});

			MethodInfo RouteMessageInfo = AccessTools.Method(typeof(RotAwayLocation), "RouteMessage");
			FieldInfo parentInfo = AccessTools.Field(typeof(ThingComp), "parent");

			foreach (CodeInstruction i in instructions)
			{
				if(i.opcode == OpCodes.Call && i.operand == MessageNoLocInfo)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);//this
					yield return new CodeInstruction(OpCodes.Ldfld, parentInfo);//this.parent
					i.operand = RouteMessageInfo;
				}
				yield return i;
			}
		}

		public static void RouteMessage(string text, MessageTypeDef type, Thing rotted)
		{
			Messages.Message(text, new GlobalTargetInfo(rotted.Position, rotted.Map), type);
		}
	}
}
