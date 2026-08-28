using EntityComponentSystemCSharp;

namespace MegaDungeon
{
	public interface IGameSession
	{
		void DoTurn(PlayerInput playerInput);

		GameStateSnapshot GetSnapshot();

		bool IsGameOver {get;}

		EntityManager.Entity PlayerEntity {get;}
	}
}
