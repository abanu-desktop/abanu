using System;
using Gtk;

namespace SharpShell.Panel
{
	
	public class DesktopWindow : Gtk.Window
	{
		
		public DesktopWindow()
			: base(WindowType.Toplevel)
		{
			Decorated = false;
			Move(0, 0);
			SetSizeRequest(Screen.Width, Screen.Height);
		}

	}

}

