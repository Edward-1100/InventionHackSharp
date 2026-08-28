using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;
using MegaDungeon.Contracts;

namespace MegaDungeon.Modes
{
	public class EndlessMode : IGameMode
	{
		public string Name => "Endless";
		int _turnsPerWave = 15;
		int _turnCount = 0;
		int _wave = 0;

		public void Initialize(IEngine engine)
		{
		}

		public void OnTurn(IEngine engine)
		{
			_turnCount++;
			if(_turnCount % _turnsPerWave == 0)
			{
				_wave++;
				SpawnWave(engine);
			}
		}

		void SpawnWave(IEngine engine)
		{
			var actorManager = new ActorManager(engine.GetEntityManager());
			var toSpawn = 2 + _wave;
			for(int i = 0; i < toSpawn; i++)
			{
				var cell = engine.GetWalkableCell();
				var monster = actorManager.CreateActor(60, "Kobold", maxHealth: 40 + (_wave * 10), defense: 8 + _wave, power: 6 + _wave, accuracy: 60 + (_wave * 2));
				monster.AddComponent(new Location(){X = cell.X, Y = cell.Y});
				monster.AddComponent(new Faction(){Type = Factions.Monster});
				monster.AddComponent<WanderingMonster>();
			}
		}

		public GameModeOutcome CheckOutcome(IEngine engine) => GameModeOutcome.InProgress;

		public string GetHudLine(IEngine engine) => $"Wave: {_wave}  Turn: {_turnCount}";
	}
}
