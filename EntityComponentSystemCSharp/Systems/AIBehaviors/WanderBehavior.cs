using System;
using System.Collections.Generic;
using EntityComponentSystemCSharp.Components;
using RogueSharp;

namespace EntityComponentSystemCSharp.Systems
{
	public class WanderBehavior : IMonsterBehavior
	{
		Random _rand = new Random();
		public string Name => "Wander";

		public void Act(IEngine engine, EntityManager.Entity monster)
		{
			var actual = monster.GetComponent<Location>();
			var desired = monster.GetComponent<Destination>();
			if(desired == null || desired == actual)
			{
				monster.RemoveComponent<Destination>();
				var walkable = new List<ICell>();
				foreach(var cell in engine.GetMap().GetAllCells())
				{
					if(cell.IsWalkable) {walkable.Add(cell);}
				}
				var randCell = walkable[_rand.Next(0, walkable.Count)];
				desired = monster.AddComponent<Destination>();
				desired.X = randCell.X;
				desired.Y = randCell.Y;
			}
		}
	}
}
