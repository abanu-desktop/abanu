using System;
using Microsoft.Win32;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using Gdk;
using Gtk;

using SharpShell.Core;

namespace SharpShell.Win32
{

	public class ShellManagerWin32 : ShellManager
	{
		private SystemProcessHookForm form;

		public ShellManagerWin32()
		{
			form = new SystemProcessHookForm();
			form.WindowEvent += (s, e) => {
				CoreLib.Log((string)e);
			};
		}

		public class SystemProcessHookForm : Form
		{
			private readonly int msgNotify;

			public delegate void EventHandler(object sender,string data);

			public event EventHandler WindowEvent;

			protected virtual void OnWindowEvent(string data)
			{
				var handler = WindowEvent;
				if (handler != null) {
					handler(this, data);
				}
			}

			public SystemProcessHookForm()
			{
				// Hook on to the shell
				msgNotify = Interop.RegisterWindowMessage("SHELLHOOK");
				Interop.RegisterShellHookWindow(this.Handle);
			}

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == msgNotify) {
					// Receive shell messages
					switch ((Interop.ShellEvents)m.WParam.ToInt32()) {
						case Interop.ShellEvents.HSHELL_WINDOWCREATED:
							ShellManager.Current.RaiseWindowCreated(ShellManager.Current.Windows.GetOrCreate(m.LParam));
							break;
						case Interop.ShellEvents.HSHELL_WINDOWDESTROYED:
							ShellManager.Current.RaiseWindowDestroyed(ShellManager.Current.Windows.GetOrCreate(m.LParam));
							break;
						case Interop.ShellEvents.HSHELL_WINDOWACTIVATED:
							ShellManager.Current.RaiseWindowActivated(ShellManager.Current.Windows.GetOrCreate(m.LParam));
							break;

					/*var wnd = new TWindowWin32(m.LParam);
							string wName = wnd.GetName();
							var action = (Interop.ShellEvents)m.WParam.ToInt32();
							OnWindowEvent(string.Format("{0} - {1}: {2}", action, m.LParam, wName));
							break;*/
					}
				}
				base.WndProc(ref m);
			}

			protected override void Dispose(bool disposing)
			{
				try {
					Interop.DeregisterShellHookWindow(this.Handle);
				} catch {
				}
				base.Dispose(disposing);
			}
		}

		public override void UpdateWindows()
		{
			Interop.EnumWindows((hwnd, lParam) => {
				TWindowWin32 wnd = (TWindowWin32)Windows.GetOrCreate(hwnd);

				if (wnd.IsAltTabWindow())
					wnd.Log();

				return true;
			}, IntPtr.Zero);
		}

	}

	public class TWindowWin32 : TWindow
	{

		public override void Minimize()
		{
			Interop.ShowWindow(hwnd, Interop.SW_MINIMIZE);	
		}

		public TWindowWin32(IntPtr hwnd)
		{
			this.hwnd = hwnd;
		}

		public bool GetIsVisible()
		{
			return Interop.IsWindowVisible(hwnd);
		}

		public override void BringToFront()
		{
			CoreLib.Log("BringToFront");
			//Interop.BringWindowToTop(hwnd);
			Interop.ShowWindow(hwnd, Interop.SW_SHOW);	
			if (Interop.IsIconic(hwnd))
				Interop.ShowWindow(hwnd, Interop.SW_RESTORE);	
			Interop.SetForegroundWindow(hwnd);
		}

		public override string GetName()
		{
			StringBuilder sb = new StringBuilder();
			int longi = Interop.GetWindowTextLength(hwnd) + 1;
			sb.Capacity = longi;
			Interop.GetWindowText(hwnd, sb, sb.Capacity);
			return sb.ToString();
		}

		public void Log()
		{
			CoreLib.Log("HWND: " + hwnd.ToString() + ", Name: " + GetName());

		}

		public bool  IsAltTabWindow()
		{
			var name = GetName();

			Interop.TITLEBARINFO ti = new Interop.TITLEBARINFO();
			IntPtr hwndTry = IntPtr.Zero;
			IntPtr hwndWalk = IntPtr.Zero;

			if (!Interop.IsWindowVisible(hwnd))
				return false;

			hwndTry = Interop.GetAncestor(hwnd, Interop.GA_ROOTOWNER);
			while (hwndTry != hwndWalk) {
				hwndWalk = hwndTry;
				hwndTry = Interop.GetLastActivePopup(hwndWalk);
				if (Interop.IsWindowVisible(hwndTry))
					break;
			}
			if (hwndWalk != hwnd)
				return false;

			// the following removes some task tray programs and "Program Manager"
			/*ti.cbSize = (uint)Marshal.SizeOf<Interop.TITLEBARINFO>();
			Interop.GetTitleBarInfo(hwnd, ref ti);
			if ((ti.rgstate[0] & Interop.STATE_SYSTEM_INVISIBLE) != 0)
				return false;*/

			// Tool windows should not be displayed either, these do not appear in the
			// task bar.
			if ((Interop.GetWindowLong(hwnd, Interop.GWL_EXSTYLE) & Interop.WS_EX_TOOLWINDOW) != 0)
				return false;

			return true;
		}

		public override bool ShowInTaskbar()
		{
			return IsAltTabWindow();
		}

		public override Image GetIcon()
		{
			var ico = Interop.GetSmallWindowIcon(hwnd);
			if (ico == null)
				return  null;
			var ms = new MemoryStream();
			ico.ToBitmap().Save(ms, System.Drawing.Imaging.ImageFormat.Png);
			ms.Position = 0;
			return new Image(ms);
		}

	}

}

