using System;
using Microsoft.Win32;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using Gdk;
using Gtk;

namespace abanu.core
{
	public abstract class ShellManager
	{

		public static ShellManager Current;

		public static ShellManager Create()
		{
			Current = Factory.Current.CreateShellManager();
			return Current;
		}

		public TWindowList Windows = new TWindowList();

		public virtual void UpdateWindows()
		{
		}

		public event Action<TWindow> WindowActivated;
		public event Action<TWindow> WindowCreated;
		public event Action<TWindow> WindowDestroyed;

		public void RaiseWindowActivated(TWindow wnd)
		{
			if (WindowActivated != null)
				WindowActivated(wnd);
		}

		public void RaiseWindowCreated(TWindow wnd)
		{
			if (WindowCreated != null)
				WindowCreated(wnd);
		}

		public void RaiseWindowDestroyed(TWindow wnd)
		{
			if (WindowDestroyed != null)
				WindowDestroyed(wnd);
		}

	}

	public class TWindowList : List<TWindow>
	{
		public TWindow GetOrCreate(IntPtr hwnd)
		{
			foreach (var wnd in this) {
				if (wnd.hwnd.Equals(hwnd))
					return wnd;
			}

			var w = TWindow.Create(hwnd);
			Add(w);

			return w;
		}
	}

	public class TWindow
	{

		public static TWindow Create(IntPtr handle)
		{
			return Factory.Current.CreateWindow(handle);
		}

		public virtual string GetName()
		{
			return "";
		}

		public virtual void BringToFront()
		{
		}

		public virtual void Minimize()
		{
		}

		public virtual bool ShowInTaskbar()
		{
			return true;
		}

		public virtual Image GetIcon(Size size)
		{
			return null;
		}

		public IntPtr hwnd;

	}

}

