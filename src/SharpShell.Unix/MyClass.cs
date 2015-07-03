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

	public class ShellManagerUnix : ShellManager
	{
		public ShellManagerUnix()
		{
			Gdk.Window.AddFilterForAll(FilterFunc);
		}

		public override void UpdateWindows()
		{
			foreach (var win in Screen.Default.WindowStack) {
				Windows.GetOrCreate(win.Handle);
			}
		}

		private Gdk.Window lastActive;

		private FilterReturn FilterFunc(IntPtr xevent, Event evnt)
		{
			if (xevent == IntPtr.Zero)
				return FilterReturn.Continue;

			var e = (X11.XEvent)Marshal.PtrToStructure(xevent, typeof(X11.XEvent));

			if (e.type == X11.XEventName.PropertyNotify) {
				var actWin = Screen.Default.ActiveWindow;

				if (lastActive == null || actWin.Handle != lastActive.Handle) {
					lastActive = actWin;
					CoreLib.Log(actWin.Height.ToString());
					CoreLib.Log(Screen.Default.WindowStack.Length.ToString());
				}

			}

			// Everything else just process as normal
			return FilterReturn.Continue;
		}

	}

	public class TWindowUnix : TWindow
	{
					
		private Gdk.Window wnd;
		private Gtk.Window wnd2;

		public TWindowUnix(IntPtr handle)
		{
			this.hwnd = handle;
			this.wnd = new Gdk.Window(handle);

			foreach (var gtkWin in Gtk.Window.ListToplevels()) {
				var s = "";
			}


			//this.wnd2 = new Gtk.Window(handle);
		}

		public override void BringToFront()
		{
			wnd.Focus(0);
		}

		public string GetPropertyString(string name)
		{
			Atom propType;
			int format;
			int length;
			byte[] data;
			if (Gdk.Property.Get(wnd, Atom.Intern(name, false), null, 0, 1000, 0, out propType, out format, out length, out data)) {
				return System.Text.Encoding.UTF8.GetString(data, 0, length);
			}
			return null;
		}

		public override string GetName()
		{
			return GetPropertyString("WM_NAME");
		}

		public string GetIconName()
		{
			return GetPropertyString("WM_ICON_NAME");
		}

		public override MemoryStream GetIcon()
		{
			//return Image.NewFromIconName("search");
			return null;
		}

	}

}

