using System;
using System.Collections.Generic;
using static Portable.MegaDungeonUIConstants;

namespace Portable
{
	public class InputHandler
	{
		MegaDungeon.PlayerInput _lastInput = MegaDungeon.PlayerInput.NONE;
		Dictionary<Inv.Key, Action> _uiCommands = new Dictionary<Inv.Key, Action>();

		public MegaDungeon.PlayerInput LastInput {get => _lastInput; set => _lastInput = value;}

		public InputHandler(Action zoomIn, Action zoomOut, Action toggleDebugInfo)
		{
			_uiCommands[Inv.Key.Plus] = zoomIn;
			_uiCommands[Inv.Key.Minus] = zoomOut;
			_uiCommands[Inv.Key.F2] = toggleDebugInfo;
		}

		public void AcceptInput(Inv.Keystroke keystroke)
		{
			if (KeyMap.ContainsKey(keystroke.Key))
			{
				_lastInput = KeyMap[keystroke.Key];
			}
			else if (_uiCommands.ContainsKey(keystroke.Key))
			{
				_uiCommands[keystroke.Key]();
			}
		}
	}
}
