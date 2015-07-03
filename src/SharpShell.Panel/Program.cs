using System;
using Gtk;

using SharpShell.Core;

namespace SharpShell.Panel
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Application.Init();



			//var dwin = new DesktopWindow();
			//dwin.Show();

			try {
				var logwin = new LogWindow();
				logwin.Show();
				LibCore.Log("log started");
				//Gtk.Settings.Default.ThemeName = "Dorian-3.16";
				var shellMan = ShellManager.Create();
				shellMan.UpdateWindows();

				var idx = new TLauncherIndex();
				idx.AddLocations();
				idx.Rebuild();

				var pwin = new TPanel();
				pwin.Setup();
				pwin.Show();
			} catch (Exception ex) {
				LibCore.MessageBox(ex.ToString());
			}

			Application.Run();
		}
	}
}
