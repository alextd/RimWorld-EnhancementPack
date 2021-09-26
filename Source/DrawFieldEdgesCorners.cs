using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace TD_Enhancement_Pack
{
	[HarmonyPatch(typeof(GenDraw), "DrawFieldEdges", new Type[] { typeof(List<IntVec3>), typeof(Color), typeof(float?) } )]
	class DrawFieldEdgesCorners
	{
		private static BoolGrid fieldGrid;

		private static bool[] adjEmpty = new bool[8];

		//public static void DrawFieldEdges(List<IntVec3> cells, Color color, float? altOffset = null)
		public static bool Prefix(List<IntVec3> cells, Color color, float? altOffset = null)
		{
			if (!Settings.Get().fieldEdgesRedo) return true;

			//TODO: Handle 1.3 altOffset
			//			float y = altOffset ?? (Rand.ValueSeeded(color.ToOpaque().GetHashCode()) * (3f / 74f) / 10f);
			//	Graphics.DrawMesh(MeshPool.plane10, c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays) + new Vector3(0f, y, 0f), new Rot4(k).AsQuat, material, 0);

			Map currentMap = Find.CurrentMap;
			MaterialRequest req = new MaterialRequest
			{
				shader = ShaderDatabase.Transparent,
				color = color,
				BaseTexPath = "TargetHighlight_Edge"
			};
			Material materialEdge = MaterialPool.MatFrom(req);
			materialEdge.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;

			req.BaseTexPath = "TargetHighlight_Edge2";
			Material materialEdge2 = MaterialPool.MatFrom(req);
			materialEdge2.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;

			req.BaseTexPath = "TargetHighlight_Edge3";
			Material materialEdge3 = MaterialPool.MatFrom(req);
			materialEdge3.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;

			req.BaseTexPath = "TargetHighlight_Edge4";
			Material materialEdge4 = MaterialPool.MatFrom(req);
			materialEdge4.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;

			req.BaseTexPath = "TargetHighlight_Corner";
			Material materialCorner = MaterialPool.MatFrom(req);
			materialCorner.GetTexture("_MainTex").wrapMode = TextureWrapMode.Clamp;
			if (fieldGrid == null)
			{
				fieldGrid = new BoolGrid(currentMap);
			}
			else
			{
				fieldGrid.ClearAndResizeTo(currentMap);
			}
			int x = currentMap.Size.x;
			int z = currentMap.Size.z;
			foreach (IntVec3 cell in cells)
				if (cell.InBounds(currentMap))
					fieldGrid[cell.x, cell.z] = true;

			foreach (IntVec3 c in cells)
				if (c.InBounds(currentMap))
				{
					adjEmpty[0] = (c.z < z - 1 && !fieldGrid[c.x, c.z + 1]);//north
					adjEmpty[1] = (c.x < x - 1 && !fieldGrid[c.x + 1, c.z]);//east
					adjEmpty[2] = (c.z > 0 && !fieldGrid[c.x, c.z - 1]);//south
					adjEmpty[3] = (c.x > 0 && !fieldGrid[c.x - 1, c.z]);//west

					adjEmpty[4] = (c.x < x - 1 && c.z < z - 1 && !fieldGrid[c.x + 1, c.z + 1]);//northeast
					adjEmpty[5] = (c.x < x - 1 && c.z > 0 && !fieldGrid[c.x + 1, c.z - 1]);//southeast
					adjEmpty[6] = (c.x > 0 && c.z > 0 && !fieldGrid[c.x - 1 , c.z - 1]);//southwest
					adjEmpty[7] = (c.x > 0 && c.z < z - 1 && !fieldGrid[c.x - 1, c.z + 1]);//northwest

					//Count # of edges empty
					int adjOrthEmpty = 0;
					for (int i = 0; i < 4; i++)
						if (adjEmpty[i])
							adjOrthEmpty++;

					Vector3 cellVector = c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
					//Draw edges using texture png based on total # of edges
					if (adjOrthEmpty == 4)
					{
						Graphics.DrawMesh(MeshPool.plane10, cellVector, Quaternion.identity, materialEdge4, 0);
					}
					else if (adjOrthEmpty == 3)
					{
						for (int i = 0; i < 4; i++)
							if (!adjEmpty[i])
								Graphics.DrawMesh(MeshPool.plane10, cellVector, new Rot4(i).AsQuat, materialEdge3, 0);
					}
					else if (adjOrthEmpty == 2)
					{
						bool corner = false;
						for (int i = 0; i < 4; i++)
						{
							if (adjEmpty[i] && adjEmpty[(i + 1) % 4])
							{
								Graphics.DrawMesh(MeshPool.plane10, cellVector, new Rot4(i).AsQuat, materialEdge2, 0);
								corner = true;
							}
						}
						//Opposite edges : just draw single edge twice
						if(!corner)
							for (int i = 0; i < 4; i++)
								if (adjEmpty[i])
									Graphics.DrawMesh(MeshPool.plane10, cellVector, new Rot4(i).AsQuat, materialEdge, 0);
					}
					else if (adjOrthEmpty == 1)
					{
						for (int i = 0; i < 4; i++)
							if (adjEmpty[i])
								Graphics.DrawMesh(MeshPool.plane10, cellVector, new Rot4(i).AsQuat, materialEdge, 0);
					}

					//Draw corner fill-ins
					for (int i = 0; i < 4; i++)
						if (adjEmpty[i+4] && !adjEmpty[i] && !adjEmpty[(i+1)%4])
							Graphics.DrawMesh(MeshPool.plane10, cellVector, new Rot4(i).AsQuat, materialCorner, 0);
				}

				return false;
		}
	}
}
