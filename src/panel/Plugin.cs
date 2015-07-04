using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

using abanu.core;

namespace abanu.panel
{

	public class TPlugin
	{
		
		public Widget widget;

	}

	public class MenuPlugin : TPlugin
	{

		private ToggleButton button;

		public MenuPlugin()
		{
			widget = button = new ToggleButton();
			button.Add(new Label("Menu"));
			button.Clicked += (s, e) => {
				if (button.Active)
					Open();
				else
					Close();
			};
		}

		private LauncherMenuWindow win;

		public void Close()
		{
			win.Hide();
		}

		public void Open()
		{
			if (win == null) {
				win = new LauncherMenuWindow(this);
			}
			win.Show();
			var alloc = button.Allocation;
			win.Move(alloc.X, alloc.Height);
		}

		public class LauncherMenuWindow : Gtk.Window
		{

			private MenuPlugin plug;

			public LauncherMenuWindow(MenuPlugin plug)
				: base(Gtk.WindowType.Toplevel)
			{
				this.plug = plug;
				SetSizeRequest(500, 500);
				var wid = new LauncherWidget();
				Add(wid);
				wid.Show();
				Decorated = false;
			}

			protected override bool OnFocusOutEvent(EventFocus evnt)
			{
				Hide();
				return base.OnFocusOutEvent(evnt);
			}

			protected override void OnHidden()
			{
				plug.button.Active = false;
				base.OnHidden();
			}

		}

	}

	public class TasksPlugin : TPlugin
	{

		private Box box;
		private Dictionary<TWindow, ToggleButton2> buthash2 = new Dictionary<TWindow, ToggleButton2>();

		public ToggleButton2 GetButton(TWindow wnd)
		{
			ToggleButton2 bt = null;
			buthash2.TryGetValue(wnd, out bt);
			return bt;
		}

		public TasksPlugin()
		{
			widget = box = new Box(Orientation.Horizontal, 0);
			Update();

			ShellManager.Current.WindowActivated += (wnd) => {
				

				var bt = GetButton(wnd);
				if (bt != null) {
					CoreLib.Log("act");
					Application.Invoke((s, e) => {
						foreach (var b in buthash2.Values) {
							b.Active = bt == b;
						}
					});
					//bt.Toggle();
				}
			};
			ShellManager.Current.WindowDestroyed += (wnd) => {
				var bt = GetButton(wnd);
				if (bt != null) {
					buthash2.Remove(wnd);
					box.Remove(bt);
					bt.Dispose();
				}
			};
			ShellManager.Current.WindowCreated += (wnd) => {
				if (wnd.ShowInTaskbar())
					createButton(wnd);
			};
		}

		public void Update()
		{
			foreach (Widget child in box) {
				box.Remove(child);
				child.Dispose();
			}

			foreach (var wnd in ShellManager.Current.Windows) {
				if (wnd.ShowInTaskbar()) {
					createButton(wnd);
				}
			}

		}

		public class ToggleButton2 : ToggleButton
		{

			public TWindow wnd;

			public ToggleButton2(TWindow wnd)
			{
				this.wnd = wnd;
				Events = EventMask.AllEventsMask;
			}

			protected override bool OnButtonPressEvent(EventButton evnt)
			{
				if (Active)
					wnd.Minimize();
				else
					wnd.BringToFront();
				return false;
			}

		}

		private void createButton(TWindow wnd)
		{
			CoreLib.Log(wnd.hwnd.ToString());

			var b = new HBox();
			b.Events = EventMask.AllEventsMask;

			var but = new ToggleButton2(wnd);
			but.Add(b);
			box.Add(but);

			var img = wnd.GetIcon();
			if (img != null) {
				img.Events = EventMask.AllEventsMask;
				//but.Image = img;
				b.Add(img);
			}

			//but.Label = wnd.GetName();
			//AppLib.log(but.Children.Length.ToString());

			var lab = new Label();
			lab.Ellipsize = Pango.EllipsizeMode.End;
			lab.MaxWidthChars = 20;
			lab.WidthChars = 20;
			lab.Text = wnd.GetName();
			lab.Events = EventMask.AllEventsMask;

			b.Add(lab);
			//but.SetSizeRequest(150, box.HeightRequest);

			b.ShowAll();

			buthash2.Add(wnd, but);

			but.Clicked += (s, e) => {
				//wnd.BringToFront();
			};

		}

	}

}

