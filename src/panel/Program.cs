using System;
using Gtk;

using abanu.core;

namespace abanu.panel
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Application.Init();



			//var dwin = new DesktopWindow();
			//dwin.Show();

			/*GLib.ExceptionManager.UnhandledException += (e) => {
				e.ExitApplication = false;
				CoreLib.Log(e.ExceptionObject.ToString());
			};*/

			try {
				var logwin = new LogWindow();
				logwin.Show();
				CoreLib.Log("log started");


				AppConfig.Load("../config/config.xml");

				//Gtk.Settings.Default.ThemeName = "Dorian-3.16";
				var shellMan = ShellManager.Create();
				shellMan.UpdateWindows();

				var idx = new TLauncherIndex();
				idx.AddLocations();
				idx.Rebuild();

				foreach (var panConfig in AppConfig.Panels) {
					var pwin = new TPanel(panConfig);
					pwin.Setup();
					pwin.Show();
				}
			} catch (Exception ex) {
				CoreLib.MessageBox(ex.ToString());
			}

			Application.Run();
		}
	}
}
