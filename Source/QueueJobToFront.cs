using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using Verse.AI;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.TryTakeOrderedJob))]
	class QueueJobToFront
	{
		//public bool TryTakeOrderedJob(Job job, JobTag? tag = 0, bool requestQueueing = false)
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			MethodInfo EnqueueLastInfo = AccessTools.Method(typeof(JobQueue), nameof(JobQueue.EnqueueLast));

			bool didOne = false;//First call to enqueue only. Technically doesn't matter since second call is after queue cleared.
			foreach(var inst in instructions)
			{
				if (!didOne && inst.Calls(EnqueueLastInfo))
				{
					didOne = true;
					inst.operand = AccessTools.Method(typeof(QueueJobToFront), nameof(EnqueueLastOrFirst));
				}
				yield return inst;
			}
		}

		//public void EnqueueLast(Job j, JobTag? tag = null)
		public static void EnqueueLastOrFirst(JobQueue queue, Job j, JobTag? tag = null)
		{
			if(KeyBindingDefOf.ModifierIncrement_10x.IsDown)//ctrl
				queue.EnqueueFirst(j, tag);
			else
				queue.EnqueueLast(j, tag);
		}
	}
}
