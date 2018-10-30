using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Harmony;

namespace TD_Enhancement_Pack
{
	abstract class BaseOverlay : ICellBoolGiver
	{
		public static Dictionary<Type, Dictionary<Map, BaseOverlay>> overlays = new Dictionary<Type, Dictionary<Map, BaseOverlay>>();

		public static HashSet<Type> toggleShow = new HashSet<Type>();

		public static BaseOverlay GetOverlay(Type type, Map map)
		{
			if (!overlays.TryGetValue(type, out Dictionary<Map, BaseOverlay> overlayDict))
			{
				overlayDict = new Dictionary<Map, BaseOverlay>();
				overlays[type] = overlayDict;
			}

			if (!overlayDict.TryGetValue(map, out BaseOverlay overlay))
			{
				overlay = Activator.CreateInstance(type, map) as BaseOverlay;
				overlayDict[map] = overlay;
			}
			return overlay;
		}
		public static List<Type> overlayTypes = typeof(BaseOverlay).AllSubclassesNonAbstract().ToList();
		public static IEnumerable<BaseOverlay> CurrentOverlays()
		{
			foreach (Type subType in overlayTypes)
				yield return BaseOverlay.GetOverlay(subType, Find.CurrentMap);
		}

		protected CellBoolDrawer drawer;
		protected Map map;
		protected float defaultOpacity;


		public BaseOverlay(Map m) : this(m, 0.33F) { }
		public BaseOverlay(Map m, float opacity)
		{
			map = m;
			defaultOpacity = opacity;
			drawer = new CellBoolDrawer((ICellBoolGiver)this, map.Size.x, map.Size.z, opacity * Settings.Get().overlayOpacity);
		}

		public Color Color
		{
			get
			{
				return Color.white;
			}
		}

		public void SetOpacity(float factor)
		{
			AccessTools.Field(typeof(CellBoolDrawer), "opacity").SetValue(drawer, defaultOpacity * factor);
			AccessTools.Field(typeof(CellBoolDrawer), "material").SetValue(drawer, null);
			drawer.SetDirty();
		}

		public static void SetAllOpacity(float factor)
		{
			foreach (var kvp in overlays)
				foreach (BaseOverlay overlay in kvp.Value.Values)
					overlay.SetOpacity(factor);
		}

		public bool GetCellBool(int index)
		{
			return !map.fogGrid.IsFogged(index) && ShowCell(index);
		}
		public virtual bool ShowCell(int index) => true;
		public abstract Color GetCellExtraColor(int index);

		public virtual void Update()
		{
			if (toggleShow.Contains(this.GetType()) || ShouldAutoDraw() && AutoDraw())
				drawer.MarkForDraw();
			drawer.CellBoolDrawerUpdate();
		}
		
		public virtual bool ShouldAutoDraw() => false;

		public void SetDirty()
		{
			drawer.SetDirty();
		}
		public static void SetDirty(Type type, Map map)
		{
			BaseOverlay.GetOverlay(type, map).SetDirty();
		}

		public virtual Type AutoDesignator() => null;
		public virtual bool DesignatorVerifier(Designator des) => true;

		public bool AutoDraw()
		{
			Designator des = Find.DesignatorManager.SelectedDesignator;
			return des != null && 
				AutoDesignator().IsAssignableFrom(des.GetType()) &&	
				DesignatorVerifier(des);
		}

		public virtual Texture2D Icon() => null;
		public virtual bool IconEnabled() => false;//from Settings
		public virtual string IconTip() => "";
	}

	[HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
	static class MapInterfaceUpdate_Patch
	{
		public static void Postfix()
		{
			if (Find.CurrentMap == null || WorldRendererUtility.WorldRenderedNow)
				return;

			foreach (BaseOverlay overlay in BaseOverlay.CurrentOverlays())
				overlay.Update();
		}
	}

	//Toggle it on buttons
	[HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
	[StaticConstructorOnStartup]
	public static class PlaySettings_Patch
	{
		[HarmonyPostfix]
		public static void AddButton(WidgetRow row, bool worldView)
		{
			if (worldView) return;

			foreach (BaseOverlay overlay in BaseOverlay.CurrentOverlays())
			{
				if (!overlay.IconEnabled()) continue;

				Type overlayType = overlay.GetType();

				bool show = BaseOverlay.toggleShow.Contains(overlayType);
				bool oldShow = show;
				row.ToggleableIcon(ref show, overlay.Icon(), overlay.IconTip());
				if (show != oldShow)
				{
					if (show) BaseOverlay.toggleShow.Add(overlayType);
					else BaseOverlay.toggleShow.Remove(overlayType);
				}
			}
		}
	}
}
