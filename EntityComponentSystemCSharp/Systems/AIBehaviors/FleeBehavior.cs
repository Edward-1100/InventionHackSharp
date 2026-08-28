using System;
using System.Collections.Generic;
using EntityComponentSystemCSharp.Components;
using RogueSharp;

namespace EntityComponentSystemCSharp.Systems
{
	public class FleeBehavior : IMonsterBehavior
	{
		Random _rand = new Random();
		int _samples = 10;
		public string Name => "Flee";

		public void Act(IEngine engine, EntityManager.Entity monster)
		{
			var playerLocation = engine.GetPlayerLocation();
			var walkable = new List<ICell>();
			foreach(var cell in engine.GetMap().GetAllCells())
			{
				if(cell.IsWalkable) {walkable.Add(cell);}
			}
			if(walkable.Count == 0) {return;}

			ICell best = null;
			var bestDist = -1;
			for(int i = 0; i < _samples; i++)
			{
				var candidate = walkable[_rand.Next(0, walkable.Count)];
				var dx = candidate.X - playerLocation.X;
				var dy = candidate.Y - playerLocation.Y;
				var dist = (dx * dx) + (dy * dy);
				if(dist > bestDist) {bestDist = dist; best = candidate;}
			}

			monster.RemoveComponent<Destination>();
			var desired = monster.AddComponent<Destination>();
			desired.X = best.X;
			desired.Y = best.Y;
		}
	}
}
