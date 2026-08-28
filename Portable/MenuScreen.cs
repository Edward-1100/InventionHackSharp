using System.Collections.Generic;
using Inv;

namespace Portable
{
	public class MenuScreen
	{
		string _title;
		List<string> _options;
		int _selectedIndex;
		Label _label;

		public Label Control => _label;

		public MenuScreen(Surface surface, string title, List<string> options, Colour textColour)
		{
			_title = title;
			_options = options;
			_selectedIndex = 0;

			_label = surface.NewLabel();
			_label.Font.Large();
			_label.Font.Colour = textColour;
			_label.Background.Colour = Colour.Black;
			_label.Justify.Center();
			Refresh();
		}

		public void SetOptions(List<string> options)
		{
			_options = options;
			_selectedIndex = 0;
			Refresh();
		}

		public void MoveUp()
		{
			_selectedIndex = (_selectedIndex - 1 + _options.Count) % _options.Count;
			Refresh();
		}

		public void MoveDown()
		{
			_selectedIndex = (_selectedIndex + 1) % _options.Count;
			Refresh();
		}

		public string GetSelected() => _options[_selectedIndex];

		void Refresh()
		{
			var lines = new List<string>();
			lines.Add(_title);
			lines.Add("");
			for(int i = 0; i < _options.Count; i++)
			{
				lines.Add((i == _selectedIndex ? "> " : "  ") + _options[i]);
			}
			lines.Add("");
			lines.Add("Up / Down to choose, Space to select");
			_label.Text = string.Join("\n", lines);
		}
	}
}
