using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(SkillUI), "DrawSkill", new Type[] { typeof(SkillRecord), typeof(Rect), typeof(SkillUI.SkillDrawMode), typeof(string) })]
	public static class SkillLearningIndicator
	{
		//public static void DrawSkill(SkillRecord skill, Rect holdingRect, SkillUI.SkillDrawMode mode, string tooltipPrefix = "")
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo EndGroupInfo = AccessTools.Method(typeof(Widgets), nameof(Widgets.EndGroup));

			MethodInfo LabelLearningInfo = AccessTools.Method(typeof(SkillLearningIndicator), nameof(LabelLearning));

			FieldInfo levelLabelWidthInfo = AccessTools.Field(typeof(SkillUI), "levelLabelWidth");

			foreach (CodeInstruction i in instructions)
			{
				if(i.Calls(EndGroupInfo))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);//SkillRecord
					yield return new CodeInstruction(OpCodes.Ldarg_1);//holdingRect
					yield return new CodeInstruction(OpCodes.Ldsfld, levelLabelWidthInfo);//levelLabelWidth
					yield return new CodeInstruction(OpCodes.Call, LabelLearningInfo);
				}
				yield return i;
			}
		}

		public static void LabelLearning(SkillRecord skillRecord, Rect holdingRect, float levelLabelWidth)
		{
			if (!Mod.settings.skillArrows) return;

			List<LearnedInfo> rec = Current.Game.GetComponent<LearnedGameComponent>().learnedInfo;
			if (rec.FirstOrDefault(i => i.record == skillRecord) is LearnedInfo info)
			{
				float skillGain = info.xp;
				if (skillGain == 0) return;
				if (skillGain > 0)
				{
					if (!Mod.settings.skillUpArrows) return;
					skillGain *= 5;
				}
				else
				{
					if (!Mod.settings.skillDownArrows) return;
					skillGain /= 10;
				}

				Color oldColor = GUI.color;

				Color arrowColor = skillGain > 0 ? Color.green : Color.red;
				arrowColor.a = Mathf.Clamp01(Math.Abs(skillGain));
				GUI.color = arrowColor;

				Rect iconRect = new Rect(Vector2.zero, Vector2.one * holdingRect.height);
				iconRect.x += levelLabelWidth;
				//Hack in the end result of LeftEdgeMargin + SkillHeight + 4 + skill # width ish
				iconRect.x += 6 + 24 + 4 + 36;

				Widgets.DrawTextureFitted(iconRect, Tex.Arrow, 1, new Vector2((float)Tex.Arrow.width, (float)Tex.Arrow.height), new Rect(0f, 0f, 1f, 1f), skillGain > 0 ? 0 : 180);

				GUI.color = oldColor;
			}
		}
	}
	[StaticConstructorOnStartup]
	public static class Tex
	{
		public static readonly Texture2D Arrow = ContentFinder<Texture2D>.Get("UI/Overlays/Arrow", true);
	}

	public class LearnedInfo
	{
		public SkillRecord record;
		public float xp;
		public int tickToKill;

		public LearnedInfo(SkillRecord r, float x, int f)
		{
			record = r;
			xp = x;
			tickToKill = f;
		}

		public override string ToString()
		{
			return String.Format("LInfo: {0}@{1}:{2}", record.ToString(), xp, tickToKill);
		}
	}

	public class LearnedGameComponent : GameComponent
	{
		public List<LearnedInfo> learnedInfo = new List<LearnedInfo>();

		public LearnedGameComponent(Game game) { }

		public override void GameComponentTick()
		{
			base.GameComponentTick();
			if (!Mod.settings.skillArrows) return;

			learnedInfo.RemoveAll(i => i.tickToKill <= GenTicks.TicksGame);
		}
	}

	[HarmonyPatch(typeof(SkillRecord), "Learn")]
	public static class Learn_Patch
	{
		//SkillRecord public void Learn(float xp, bool direct = false)
		public static void Postfix(SkillRecord __instance, float xp)
		{
			if (!Mod.settings.skillArrows) return;

			List<LearnedInfo> rec = Current.Game.GetComponent<LearnedGameComponent>().learnedInfo;

			int killAt = (GenTicks.TicksGame + 200);// loss ticks every 200, so this is fine
			if (rec.FirstOrDefault(i => i.record == __instance) is LearnedInfo info)
			{
				info.tickToKill = killAt;
				info.xp = xp;
			}
			else
				rec.Add(new LearnedInfo(__instance, xp, killAt));
		}
	}
}
