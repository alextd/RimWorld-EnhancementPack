using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Harmony;

namespace TD_Enhancement_Pack
{
	abstract class BaseOverlay : ICellBoolGiver
	{
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
				return new Color(1f, 1f, 1f);
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
			foreach (BaseOverlay overlay in LightingOverlay.lightingOverlays.Values)
				overlay.SetOpacity(factor);
			foreach (BaseOverlay overlay in BuildableOverlay.buildableOverlays.Values)
				overlay.SetOpacity(factor);
			foreach (BaseOverlay overlay in FertilityOverlay.fertilityOverlays.Values)
				overlay.SetOpacity(factor);
			foreach (BaseOverlay overlay in BeautyOverlay.beautyOverlays.Values)
				overlay.SetOpacity(factor);
		}

		public abstract bool GetCellBool(int index);
		public abstract Color GetCellExtraColor(int index);

		public void Draw()
		{
			if (ShouldDraw() || ShouldAutoDraw() && AutoDraw())
				drawer.MarkForDraw();
			drawer.CellBoolDrawerUpdate();
		}

		public virtual bool ShouldDraw() => false;
		public virtual bool ShouldAutoDraw() => false;

		public void SetDirty()
		{
			drawer.SetDirty();
		}

		public virtual Type AutoDesignator() => null;

		public bool AutoDraw()
		{
			return Find.DesignatorManager.SelectedDesignator != null &&
				AutoDesignator().IsAssignableFrom(Find.DesignatorManager.SelectedDesignator.GetType());
		}
	}
}
