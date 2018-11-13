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
	[StaticConstructorOnStartup]
	abstract class BaseOverlay : ICellBoolGiver
	{
		public static Dictionary<Type, BaseOverlay> overlays = new Dictionary<Type, BaseOverlay>();

		public static BaseOverlay GetOverlay(Type type)
		{
			if (!overlays.TryGetValue(type, out BaseOverlay overlay))
			{
				overlay = Activator.CreateInstance(type) as BaseOverlay;
				overlays[type] = overlay;
			}
			return overlay;
		}

		public static List<Type> overlayTypes = typeof(BaseOverlay).AllSubclassesNonAbstract().ToList();
		public static IEnumerable<BaseOverlay> AllOverlays()
		{
			foreach (Type subType in overlayTypes)
				yield return BaseOverlay.GetOverlay(subType);
		}

		protected float defaultOpacity;


		public BaseOverlay() : this(0.33F) { }
		public BaseOverlay(float opacity)
		{
			defaultOpacity = opacity;
		}

		public Color Color
		{
			get
			{
				return Color.white;
			}
		}

		protected CellBoolDrawer drawer;
		public void MakeDrawer()
		{
			drawer = new CellBoolDrawer((ICellBoolGiver)this, Find.CurrentMap.Size.x, Find.CurrentMap.Size.z, defaultOpacity * Settings.Get().overlayOpacity);
		}

		public static void ResetAll()
		{
			foreach (BaseOverlay overlay in overlays.Values)
				if(overlay.drawer != null)
					overlay.MakeDrawer();
		}

		public bool GetCellBool(int index)
		{
			return !Find.CurrentMap.fogGrid.IsFogged(index) && ShowCell(index);
		}
		public virtual bool ShowCell(int index) => true;
		public abstract Color GetCellExtraColor(int index);
		

		public static HashSet<Type> toggleShow = new HashSet<Type>();
		public virtual void Update()
		{
			if (toggleShow.Contains(this.GetType()) || ShouldAutoDraw() && AutoDraw())
			{
				if (drawer == null)
					MakeDrawer();

				drawer.MarkForDraw();// can't just call ActuallyDraw :/
				drawer.CellBoolDrawerUpdate();
				PostDraw();
			}
			else
				drawer = null;
		}

		public virtual void PostDraw() { }
		
		public virtual bool ShouldAutoDraw() => false;

		public void SetDirty()
		{
			drawer?.SetDirty();
		}
		public static void SetDirty(Type type)
		{
			BaseOverlay.GetOverlay(type).SetDirty();
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

	[HarmonyPatch(typeof(Game), "CurrentMap", MethodType.Setter)]
	static class ChangeMapResetOverlays
	{
		public static void Postfix()
		{
			BaseOverlay.ResetAll();
		}
	}

	[HarmonyPatch(typeof(MapInterface), "MapInterfaceUpdate")]
	static class MapInterfaceUpdate_Patch
	{
		public static void Postfix()
		{
			if (Find.CurrentMap == null || WorldRendererUtility.WorldRenderedNow)
				return;

			foreach (BaseOverlay overlay in BaseOverlay.AllOverlays())
				overlay.Update();
		}
	}

	//Toggle it on buttons
	[HarmonyPatch(typeof(PlaySettings), "DoPlaySettingsGlobalControls")]
	public static class PlaySettings_Patch
	{
		[HarmonyPostfix]
		public static void AddButton(WidgetRow row, bool worldView)
		{
			if (worldView) return;

			foreach (BaseOverlay overlay in BaseOverlay.AllOverlays())
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
