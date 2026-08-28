using System;
using System.Collections.Generic;
using System.Linq;
using static Portable.MegaDungeonUIConstants;
using Inv;

namespace Portable
{
	public class MegaDungeonUI
	{
		int _horizontalCellCount;
		int _verticalCellCount;
		Surface _surface;
		TileManager _tileManager;

		AppState _state = AppState.MainMenu;
		string _selectedMode;
		string _selectedClass;

		MenuScreen _mainMenu;
		MenuScreen _modeSelect;
		MenuScreen _classSelect;
		MenuScreen _gameOverMenu;
		MenuScreen _winMenu;
		Label _quitScreen;

		MegaDungeon.IGameSession _engine;
		InputHandler _inputHandler;
		Renderer _renderer;
		Action _updateAction;
		Dock _outerDock;
		Dock _innerDock;
		Dock _rightDock;
		Stack _leftStack;
		Label _bottomLabel;
		Label _floorTurnLabel;
		Label _messageLabel;
		Scroll _messageScroll;
		Label _objectiveLabel;
		EntityData _leftPanel;

		public MegaDungeonUI(Surface surface, int horizontalCellCount, int verticalCellCount)
		{
			_horizontalCellCount = horizontalCellCount;
			_verticalCellCount = verticalCellCount;
			_surface = surface;
			_surface.ArrangeEvent += () => {
				if(_rightDock != null) {_rightDock.Size.SetWidth(_surface.Window.Width / 4);}
				if(_leftStack != null) {_leftStack.Size.SetWidth(_surface.Window.Width / 4);}
			};

			var directory = surface.Window.Application.Directory;
			_tileManager = new TileManager(directory.NewAsset("absurd64.bmp"), directory.NewAsset("tiledata.json"));

			_mainMenu = new MenuScreen(surface, "Dungeon Game", new List<string>(){"Start", "Quit"}, Colour.White);
			_modeSelect = new MenuScreen(surface, "Choose Game Mode", MegaDungeon.GameModeRegistry.CreateDefault().GetNames().ToList(), Colour.White);
			_classSelect = new MenuScreen(surface, "Choose A Class", MegaDungeon.ClassRegistry.CreateDefault().GetNames().ToList(), Colour.White);
			_gameOverMenu = new MenuScreen(surface, "You Died", new List<string>(){"Retry", "Change Class", "Change Mode", "Main Menu"}, Colour.Red);
			_winMenu = new MenuScreen(surface, "Victory!", new List<string>(){"Retry", "Change Class", "Change Mode", "Main Menu"}, Colour.YellowGreen);

			_quitScreen = surface.NewLabel();
			_quitScreen.Text = "Thanks for playing.";
			_quitScreen.Font.ExtraMassive();
			_quitScreen.Font.Colour = Colour.White;
			_quitScreen.Background.Colour = Colour.Black;
			_quitScreen.Justify.Center();

			ShowMainMenu();
		}

		void ShowMainMenu()
		{
			_state = AppState.MainMenu;
			_surface.Content = _mainMenu.Control;
		}

		void ShowModeSelect()
		{
			_state = AppState.ModeSelect;
			_surface.Content = _modeSelect.Control;
		}

		void ShowClassSelect()
		{
			_state = AppState.ClassSelect;
			_surface.Content = _classSelect.Control;
		}

		void ShowGameOver()
		{
			_state = AppState.GameOver;
			_surface.Content = _gameOverMenu.Control;
		}

		void ShowWin()
		{
			_state = AppState.Win;
			_surface.Content = _winMenu.Control;
		}

		void StartSession()
		{
			_state = AppState.Playing;

			if(_updateAction != null) {_surface.ComposeEvent -= _updateAction;}

			_engine = new MegaDungeon.Engine(_horizontalCellCount, _verticalCellCount, _tileManager, _selectedMode, _selectedClass);
			_renderer = new Renderer(_tileManager, _horizontalCellCount, _verticalCellCount, _surface.Window.Width);
			_inputHandler = new InputHandler(_renderer.ZoomIn, _renderer.ZoomOut, _renderer.ToggleDebugInfo);
			_updateAction = () => Update();

			BuildPlayingLayout();

			_surface.Content = _outerDock;
			_surface.ComposeEvent += Warmup();
		}

		void BuildPlayingLayout()
		{
			_outerDock = _surface.NewDock(Inv.Orientation.Vertical);
			_innerDock = _surface.NewDock(Inv.Orientation.Horizontal);

			_bottomLabel = InitLabel("Arrows: move   Space: wait   F2: debug view   +/-: zoom", ColorPallette.Secondary1ColorLightest, ColorPallette.Secondary1ColorDarkest, ColorPallette.Secondary1ColorDarker);
			_outerDock.AddClient(_innerDock);
			_outerDock.AddFooter(_bottomLabel);

			_leftStack = _surface.NewVerticalStack();
			_leftStack.Background.Colour = ColorPallette.Secondary1ColorDarkest;
			_leftStack.Border.Set(5);
			_leftStack.Border.Colour = ColorPallette.Secondary1ColorDarkest;
			_leftStack.Size.SetWidth(_surface.Window.Width / 4);

			_leftPanel = new EntityData(_surface, ColorPallette.Secondary1ColorLightest, ColorPallette.Secondary1ColorDarkest);
			_leftStack.AddPanel(_leftPanel.Table);

			_floorTurnLabel = InitLabel("Floor 1 | Turn 0", ColorPallette.ComplementColorLightest, ColorPallette.ComplementColorDarkest, ColorPallette.ComplementColorDarker);
			_messageLabel = InitLabel("", ColorPallette.ComplementColorLightest, ColorPallette.ComplementColorDarkest, ColorPallette.ComplementColorDarker);
			_messageLabel.LineWrapping = true;
			_messageScroll = _surface.NewVerticalScroll();
			_messageScroll.Content = _messageLabel;
			_objectiveLabel = InitLabel("", ColorPallette.ComplementColorLightest, ColorPallette.ComplementColorDarkest, ColorPallette.ComplementColorDarker);

			_rightDock = _surface.NewDock(Inv.Orientation.Vertical);
			_rightDock.Size.SetWidth(_surface.Window.Width / 4);
			_rightDock.AddHeader(_floorTurnLabel);
			_rightDock.AddClient(_messageScroll);
			_rightDock.AddFooter(_objectiveLabel);

			_innerDock.AddHeader(_leftStack);
			_innerDock.AddClient(_renderer.Control);
			_innerDock.AddFooter(_rightDock);
		}

		/// <summary>
		/// Check for full renderer init before going to usual update
		/// </summary>
		/// <returns>null</returns>
		/// <remarks>
		/// Update gets called a few times before the window is fully laid out.
		/// When base dimensions are available center the player and connect main Update();
		/// (Cannot use a lambda because -= won't be able to unregister without a method handle.)
		/// </remarks>
		System.Action Warmup()
		{
			return new Action( () =>
			{
				if(!_renderer.IsReady)
				{
					return;
				}
				_surface.ComposeEvent -= Warmup();
				var snapshot = _engine.GetSnapshot();
				_renderer.UpdateSnapshot(snapshot);
				_renderer.SetPanningXY(snapshot.PlayerLocation.X, snapshot.PlayerLocation.Y);
				_floorTurnLabel.Text = $"Floor 1 | Turn {snapshot.TurnNumber}";
				_messageLabel.Text = string.Join("\n", snapshot.Messages);
				_objectiveLabel.Text = snapshot.ModeHudLine;
				_leftPanel.SetData(_engine.PlayerEntity);
				_surface.ComposeEvent += _updateAction;
				_renderer.Draw();
			});
		}

		/// <summary>
		/// Update one atomic unit of the UI.
		/// </summary>
		/// <remarks>
		/// This method is the pump for the game. It should be fast enough to always return in less than
		/// one frame (1/60th sec.)
		/// </remarks>
		void Update()
		{
			if (_inputHandler.LastInput != MegaDungeon.PlayerInput.NONE)
			{
				if(_engine.IsGameOver)
				{
					var endSnapshot = _engine.GetSnapshot();
					_inputHandler.LastInput = MegaDungeon.PlayerInput.NONE;
					_surface.ComposeEvent -= _updateAction;
					if(endSnapshot.ModeOutcome == MegaDungeon.GameModeOutcome.Won) {ShowWin();}
					else {ShowGameOver();}
					return;
				}
				_engine.DoTurn(_inputHandler.LastInput);
				var snapshot = _engine.GetSnapshot();
				_renderer.UpdateSnapshot(snapshot);
				_renderer.KeepXYOnScreen(snapshot.PlayerLocation.X, snapshot.PlayerLocation.Y);
				_floorTurnLabel.Text = $"Floor 1 | Turn {snapshot.TurnNumber}";
				_messageLabel.Text = string.Join("\n", snapshot.Messages);
				_objectiveLabel.Text = snapshot.ModeHudLine;
				_leftPanel.SetData(_engine.PlayerEntity);
				_renderer.Draw();
				_inputHandler.LastInput = MegaDungeon.PlayerInput.NONE;
			}
		}

		public void AcceptInput(Inv.Keystroke keystroke)
		{
			switch(_state)
			{
				case AppState.MainMenu: HandleMenuInput(keystroke, _mainMenu, OnMainMenuSelect); break;
				case AppState.ModeSelect: HandleMenuInput(keystroke, _modeSelect, OnModeSelect); break;
				case AppState.ClassSelect: HandleMenuInput(keystroke, _classSelect, OnClassSelect); break;
				case AppState.GameOver: HandleMenuInput(keystroke, _gameOverMenu, OnEndScreenSelect); break;
				case AppState.Win: HandleMenuInput(keystroke, _winMenu, OnEndScreenSelect); break;
				case AppState.Playing: _inputHandler.AcceptInput(keystroke); break;
			}
		}

		void HandleMenuInput(Inv.Keystroke keystroke, MenuScreen menu, Action<string> onSelect)
		{
			if(keystroke.Key == Inv.Key.Up) {menu.MoveUp();}
			else if(keystroke.Key == Inv.Key.Down) {menu.MoveDown();}
			else if(keystroke.Key == Inv.Key.Space) {onSelect(menu.GetSelected());}
		}

		void OnMainMenuSelect(string option)
		{
			if(option == "Start") {ShowModeSelect();}
			else if(option == "Quit") {_surface.Content = _quitScreen;}
		}

		void OnModeSelect(string option)
		{
			_selectedMode = option;
			ShowClassSelect();
		}

		void OnClassSelect(string option)
		{
			_selectedClass = option;
			StartSession();
		}

		void OnEndScreenSelect(string option)
		{
			if(option == "Retry") {StartSession();}
			else if(option == "Change Class") {ShowClassSelect();}
			else if(option == "Change Mode") {ShowModeSelect();}
			else if(option == "Main Menu") {ShowMainMenu();}
		}

		Label InitLabel(string text, Colour fontColor,  Colour border, Colour background)
		{
			var newLabel = _surface.NewLabel();
			newLabel.Text = text;
			newLabel.Background.Colour = background;
			newLabel.Font.Colour = fontColor;
			newLabel.Border.Colour = border;
			newLabel.Border.Set(1);
			return newLabel;
		}
	}
}
