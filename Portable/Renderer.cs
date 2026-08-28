using System.Collections.Generic;
using Inv;

namespace Portable
{
	public class Renderer
	{
		int _horizontalCellCount;
		TileManager _tileManager;
		Cloth _cloth;
		bool _showDebugInfo = false;

		MegaDungeon.GameStateSnapshot _snapshot;
		Dictionary<int, MegaDungeon.ActorSnapshot> _actorsByLocation = new Dictionary<int, MegaDungeon.ActorSnapshot>();

		internal Cloth Control => _cloth;

		public bool IsReady => _cloth.BaseDimension.Height != 0;

		public Renderer(TileManager tileManager, int horizontalCellCount, int verticalCellCount, int windowWidth)
		{
			_tileManager = tileManager;
			_horizontalCellCount = horizontalCellCount;

			_cloth = new Cloth();
			_cloth.Dimension = new Inv.Dimension(horizontalCellCount, verticalCellCount);
			_cloth.CellSize = (windowWidth / horizontalCellCount) * 2;
			_cloth.Draw();
			_cloth.DrawEvent += (dc, patch) => Cloth_DrawEvent(dc, patch);
		}

		public void UpdateSnapshot(MegaDungeon.GameStateSnapshot snapshot)
		{
			_snapshot = snapshot;
			_actorsByLocation.Clear();
			foreach(var actor in _snapshot.Actors)
			{
				_actorsByLocation[actor.X + (actor.Y * _horizontalCellCount)] = actor;
			}
		}

		public void Draw() => _cloth.Draw();
		public void SetPanningXY(int x, int y) => _cloth.SetPanningXY(x, y);
		public void KeepXYOnScreen(int x, int y) => _cloth.KeepXYOnScreen(x, y);
		public void ZoomIn() => _cloth.Zoom(0, 0, 1);
		public void ZoomOut() => _cloth.Zoom(0, 0, -1);
		public void ToggleDebugInfo()
		{
			_showDebugInfo = !_showDebugInfo;
			_cloth.Draw();
		}

		void Cloth_DrawEvent(DrawContract dc, Patch patch)
		{
			if(_snapshot == null) {return;}

			var point = new RogueSharp.Point(patch.X, patch.Y);
			MegaDungeon.Contracts.TileType tileType = _showDebugInfo
				? _snapshot.RevealedTiles[patch.X, patch.Y]
				: _snapshot.Tiles[patch.X, patch.Y];
			int glyph = _tileManager.GetGlyphForTileType(tileType);

			Inv.Image image;
			if(_snapshot.Viewable.Contains(point) || _showDebugInfo)
			{
				var key = patch.X + (patch.Y * _horizontalCellCount);
				if(_actorsByLocation.TryGetValue(key, out var actorHere))
				{
					glyph = actorHere.Glyph;
					image = _tileManager.GetInvImage(glyph);
					dc.DrawImage(image, patch.Rect);
					if(_showDebugInfo)
					{
						DrawText(actorHere.EntityId.ToString(), dc, patch);
					}
				}
				else
				{
					image = _tileManager.GetInvImage(glyph);
					dc.DrawImage(image, patch.Rect);
				}
			}
			else
			{
				image = _tileManager.GetInvImageDark(glyph);
				dc.DrawImage(image, patch.Rect);
			}
		}

		void DrawText(string text, DrawContract dc, Patch patch)
		{
			dc.DrawText(text, "Courier", 11, FontWeight.Regular, Colour.YellowGreen, new Point(patch.Rect.Left, patch.Rect.Top), HorizontalPosition.Left, VerticalPosition.Top);
		}
	}
}
