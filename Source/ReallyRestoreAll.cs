using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Verse;
using RimWorld;
using Harmony;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(Dialog_Options), nameof(Dialog_Options.DoWindowContents))]
	class ReallyRestoreAll
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> inst)
		{
			return Harmony.Transpilers.MethodReplacer(inst,
				AccessTools.Method(typeof(Dialog_Options), nameof(Dialog_Options.RestoreToDefaultSettings)),
				AccessTools.Method(typeof(ReallyRestoreAll), nameof(ConfirmRestoreToDefaultSettings))
				);
		}

		public static void ConfirmRestoreToDefaultSettings(Dialog_Options dialog)
		{
			Find.WindowStack.Add(new Dialog_MessageBox("Really restore all settings?", buttonBText: "CancelButton".Translate(), 
				buttonAAction: () => dialog.RestoreToDefaultSettings()));
		}
	}

	[HarmonyPatch(typeof(Dialog_Options), nameof(Dialog_Options.RestoreToDefaultSettings))]
	public static class BackupConfig
	{
		public static void Prefix()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(GenFilePaths.ConfigFolderPath);
			DirectoryInfo backupDirInfo = new DirectoryInfo(GenFilePaths.ConfigFolderPath + " - backup");
			backupDirInfo.Create();
			FileInfo[] files = directoryInfo.GetFiles("*.xml");
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo fileInfo = files[i];
				try
				{
					Log.Message($"Copying {fileInfo} to {backupDirInfo.FullName + "/" + fileInfo.Name}");
					fileInfo.CopyTo(backupDirInfo.FullName + "/" + fileInfo.Name, true);
				}
				catch (SystemException e)
				{
					Log.Message($"Failed Copying {fileInfo} to {backupDirInfo.FullName + "/" + fileInfo.Name} - Exception: {e}");
				}
			}
		}
	}
}
