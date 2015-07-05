using System;
using Gtk;
using System.Diagnostics;

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
				Environment.CurrentDirectory = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(new Uri(typeof(MainClass).Assembly.CodeBase).LocalPath)).Parent.FullName;
				if (args.Length > 0) {
					if (args[0] == "--kill" || args[0] == "--replace") {
						var currProcess = Process.GetCurrentProcess();
						foreach (var process in System.Diagnostics.Process.GetProcessesByName("abanu.panel"))
							if (process.Id != currProcess.Id)
								process.Kill();
						
						if (args[0] == "--kill")
							return;
					}
				}

				var logwin = new LogWindow();
				logwin.Show();
				CoreLib.Log("log started");

				AppConfig.Load("config/config.xml");

				if (Environment.OSVersion.Platform != PlatformID.Unix) {
					AppLib.GlobalCssProvider = new CssProvider();
					AppLib.GlobalCssProvider.LoadFromPath("res/theme/gtk.css");
					StyleContext.AddProviderForScreen(Gdk.Screen.Default, AppLib.GlobalCssProvider, uint.MaxValue - 10);

					var styleWin = new StyleWindow();
					styleWin.Show();
				}

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
