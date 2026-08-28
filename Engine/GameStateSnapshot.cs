using System.Collections.Generic;
using MegaDungeon.Contracts;
using RogueSharp;

namespace MegaDungeon
{
	public class ActorSnapshot {
		public int EntityId;
		public int X;
		public int Y;
		public int Glyph;
	}

	public class GameStateSnapshot {
		public TileType[,] Tiles;
		public TileType[,] RevealedTiles;
		public HashSet<Point> Viewable;
		public Point PlayerLocation;
		public string[] Messages;
		public List<ActorSnapshot> Actors;
		public string ModeHudLine;
		public GameModeOutcome ModeOutcome;
		public int TurnNumber;
	}
}
