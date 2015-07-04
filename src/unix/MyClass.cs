using System;
using System.IO;
using Gdk;
using Gtk;
using System.Runtime.InteropServices;

using SharpShell.Core;

namespace SharpShell.Unix
{
	public class FactoryActivator : SharpShell.Core.FactoryActivator
	{
		
		public override ShellManager CreateShellManager()
		{
			return new ShellManagerUnix();
		}

		public override TWindow CreateWindow(IntPtr handle)
		{
			return new TWindowUnix(handle);
		}

		public override TLauncherEntry ReadLinkFile(string file)
		{
			var ini = new INIFile(file);
			var entry = new TLauncherEntry();
			entry.Name = ini.GetValue("Desktop Entry", "Name", "");
			var cmd = ini.GetValue("Desktop Entry", "Exec", "");
			entry.CommandPath = Path.GetDirectoryName(cmd);
			entry.CommandFile = Path.GetFileName(cmd);
			entry.CommandArgs = ""; //TODO
			entry.Categories = ini.GetValue("Desktop Entry", "Categories", "");
			entry.IconName = ini.GetValue("Desktop Entry", "Icon", "");
			entry.Description = ini.GetValue("Desktop Entry", "Comment", "");
			entry.UpdateMainCategory();
			return entry;
		}

	}

}

