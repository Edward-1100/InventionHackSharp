using EntityComponentSystemCSharp;

namespace MegaDungeon
{
	public enum GameModeOutcome {InProgress, Won, Lost}

	public interface IGameMode
	{
		string Name {get;}
		void Initialize(IEngine engine);
		void OnTurn(IEngine engine);
		GameModeOutcome CheckOutcome(IEngine engine);
		string GetHudLine(IEngine engine);
	}
}
