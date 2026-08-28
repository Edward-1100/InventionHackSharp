using System;
using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;

namespace MegaDungeon.Classes
{
	public class WarriorClass : IPlayerClass
	{
		public string Name => "Warrior";
		Random _rand = new Random();

		public ClassStats GetStats() => new ClassStats()
		{
			MaxHealth = 16,
			Defense = 14,
			Power = 10,
			Accuracy = 75,
			Speed = 7,
			SightRange = 5,
		};

		public void BeforeTurn(IEngine engine, EntityManager.Entity player)
		{
		}

		public void AfterTurn(IEngine engine, EntityManager.Entity player)
		{
			var life = player.GetComponent<Life>();
			if(life == null || life.Health >= life.MaxHealth) {return;}
			if(_rand.Next(1000) < life.MaxHealth) {life.Health++;}
		}
	}
}
