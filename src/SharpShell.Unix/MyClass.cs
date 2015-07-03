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
			using (var prop = Atom.Intern(name, false))
				if (Gdk.Property.Get(wnd, prop, null, 0, uint.MaxValue, 0, out propType, out format, out length, out data)) {
					return System.Text.Encoding.UTF8.GetString(data, 0, length);
				}
			return null;
		}

		//[DllImport("libX11", EntryPoint = "XGetWindowProperty")]
		//public extern static int XGetWindowProperty(IntPtr display, IntPtr window, IntPtr atom, IntPtr long_offset, IntPtr long_length, bool delete, IntPtr req_type, out IntPtr actual_type, out int actual_format, out IntPtr nitems, out IntPtr bytes_after, out IntPtr prop);

		public bool GetPropertyBinary2(string name, out int[] data)
		{
			using (var prop = Atom.Intern(name, false))
				return Gdk.Property.Get(wnd, prop, false, out data);
		}

		public override string GetName()
		{
			var name = GetPropertyString("WM_ICON_NAME");
			if (name == null)
				return "";
			else
				return name;
		}

		/*public string GetIconName()
		{
			return GetPropertyString("_NET_WM_ICON");
		}*/

		public override  Image GetIcon()
		{
			int[] data;
			GetPropertyBinary2("_NET_WM_ICON", out data);

			if (data != null) {
				int width = 1000;
				int height = 1000;
				int start = 0;
				int i = 0;
				while (i < data.Length) {
					if (data[i] < width && data[i + 1] < height) {
						width = data[i];
						height = data[i + 1];
						start = i;
					}
					i = i + 2 + data[i] * data[i + 1];
				}

				//try {
				var pixelLen = width * height;
				var byteLen = pixelLen * 4;
				var neData = new int[pixelLen];
				Array.Copy(data, start + 2, neData, 0, pixelLen);

				var byteArray = new byte[byteLen];
				Buffer.BlockCopy(neData, 0, byteArray, 0, byteArray.Length);

				var byteArray2 = new byte[byteLen];

				for (var n = 0; n < byteLen; n += 4) {
					byteArray2[n + 0] = byteArray[n + 2];
					byteArray2[n + 1] = byteArray[n + 1];
					byteArray2[n + 2] = byteArray[n + 0];
					byteArray2[n + 3] = byteArray[n + 3];
				}

				var pbuf = new Pixbuf(byteArray2, Colorspace.Rgb, true, 8, width, height, width * 4);
				return new Image(pbuf);
				//} catch (Exception ex) {
				//}
			}
			return null;
		}

	}

}

