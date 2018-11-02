using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Reflection;
using Verse;
using Harmony;

namespace TD.Utilities
{
	class HugsLibUpdateNews
	{
		public const string UpdateNewsFileDir = "About";
		public const string UpdateNewsFileName = "UpdateNews.xml";
		
		public static void MakeNews(Mod mod)
		{
			Type typeUpdateFeatureDef = AccessTools.TypeByName("UpdateFeatureDef");
			if (typeUpdateFeatureDef == null) return;

			var filePath = Path.Combine(mod.Content.RootDir, Path.Combine(UpdateNewsFileDir, UpdateNewsFileName));
			if (!File.Exists(filePath)) return;


			FieldInfo modNameReadableField = AccessTools.Field(typeUpdateFeatureDef, "modNameReadable");
			FieldInfo modIdentifierField = AccessTools.Field(typeUpdateFeatureDef, "modIdentifier");
			FieldInfo assemblyVersionField = AccessTools.Field(typeUpdateFeatureDef, "assemblyVersion");
			FieldInfo contentField = AccessTools.Field(typeUpdateFeatureDef, "content");
			FieldInfo linkUrlField = AccessTools.Field(typeUpdateFeatureDef, "linkUrl");
			FieldInfo defNameField = AccessTools.Field(typeUpdateFeatureDef, "defName");


			Type dbType = typeof(DefDatabase<>).MakeGenericType(new Type[] { typeUpdateFeatureDef });
			MethodInfo addMethod = AccessTools.Method(dbType, "Add", new Type[] { typeUpdateFeatureDef });

			try
			{
				XDocument doc = XDocument.Load(filePath);
				if (doc.Root == null) throw new Exception("Missing root node");
				foreach (XElement node in doc.Root.Elements("li"))
				{
					var assemblyVersion = node.Element("assemblyVersion");
					var content = node.Element("content");
					var linkUrl = node.Element("linkUrl");


					object updateDef = Activator.CreateInstance(typeUpdateFeatureDef);
					modNameReadableField.SetValue(updateDef, mod.Content.Name);
					modIdentifierField.SetValue(updateDef, mod.Content.Identifier);
					assemblyVersionField.SetValue(updateDef, assemblyVersion.Value);
					contentField.SetValue(updateDef, content.Value);
					linkUrlField.SetValue(updateDef, linkUrl.Value);
					defNameField.SetValue(updateDef, (mod.Content.Name + assemblyVersion.Value).Replace(" ", "").Replace(".", "_"));
					addMethod.Invoke(null, new object[] { updateDef });
				}
			}
			catch (Exception e)
			{
				Log.Warning($"Opening XML failed: {filePath}, Exception: {e}");
			}
		}
	}
}
