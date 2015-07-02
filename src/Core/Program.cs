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

			try {
				var logwin = new LogWindow();
				logwin.Show();
				AppLib.log("log started");

				var shellMan = ShellManager.Create();
				shellMan.UpdateWindows();

				var idx = new TLauncherIndex();
				idx.AddLocations();
				idx.Rebuild();

				var pwin = new TPanel();
				pwin.Setup();
				pwin.Show();
			} catch (Exception ex) {
				new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, ex.ToString()).Show();
			}

			Application.Run();
		}
	}
}
