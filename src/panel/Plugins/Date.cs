using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

using abanu.core;

namespace abanu.panel
{
	public class DatePlugin : TPlugin
	{

		private ToggleButton button;

		public DatePlugin(TPanel panel)
			: base(panel)
		{
			widget = button = new ToggleButton();
			button.Add(new Label("DateTime"));
			/*			button.Clicked += (s, e) => {
				if (button.Active)
					Open();
				else
					Close();
			};*/
		}

		//private LauncherMenuWindow win;

		//		public void Close()
		//		{
		//			win.Hide();
		//		}
		//
		//		public void Open()
		//		{
		//			if (win == null) {
		//				win = new LauncherMenuWindow(this);
		//			}
		//			win.Show();
		//			var alloc = button.Allocation;
		//			win.Move(alloc.X, alloc.Height);
		//		}
		//
		//		public class LauncherMenuWindow : Gtk.Window
		//		{
		//
		//			private MenuPlugin plug;
		//
		//			public LauncherMenuWindow(MenuPlugin plug)
		//				: base(Gtk.WindowType.Toplevel)
		//			{
		//				this.plug = plug;
		//				SetSizeRequest(500, 500);
		//				var wid = new LauncherWidget();
		//				Add(wid);
		//				wid.Show();
		//				Decorated = false;
		//			}
		//
		//			protected override bool OnFocusOutEvent(EventFocus evnt)
		//			{
		//				Hide();
		//				return base.OnFocusOutEvent(evnt);
		//			}
		//
		//			protected override void OnHidden()
		//			{
		//				plug.button.Active = false;
		//				base.OnHidden();
		//			}
		//
		//		}

	}

}

