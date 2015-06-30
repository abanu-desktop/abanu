using System;
using Gtk;
using Gdk;

namespace PanelShell
{

	public class TPlugin
	{
		
		public Widget widget;

	}

	public class MenuPlugin : TPlugin
	{

		private Button button;

		public MenuPlugin()
		{
			widget = button = new Button();
			button.Add(new Label("Menu"));
			button.Clicked += (s, e) => {
				
			};
		}

	}

}

