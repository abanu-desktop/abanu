using System;
using Gtk;

namespace PanelShell
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Application.Init();



			//var dwin = new DesktopWindow();
			//dwin.Show();

			var pwin = new TPanel();
			pwin.Setup();
			pwin.Show();

			Application.Run();
		}
	}
}
