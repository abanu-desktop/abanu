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

			var logwin = new LogWindow();
			logwin.Show();
			AppLib.log("log started");

			var shellMan = ShellManager.Create();
			shellMan.UpdateWindows();

			var pwin = new TPanel();
			pwin.Setup();
			pwin.Show();

			Application.Run();
		}
	}
}
