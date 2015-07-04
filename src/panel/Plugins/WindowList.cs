using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

using abanu.core;

namespace abanu.panel
{

	public class TasksPlugin : TPlugin
	{

		private PanelButtonContainer helper;
		private Dictionary<TWindow, TWindowButton> buthash2 = new Dictionary<TWindow, TWindowButton>();

		public TWindowButton GetButton(TWindow wnd)
		{
			TWindowButton bt = null;
			buthash2.TryGetValue(wnd, out bt);
			return bt;
		}

		public TasksPlugin(TPanel panel)
			: base(panel)
		{
			helper = new PanelButtonContainer(Orientation.Horizontal);
			//box.HeightRequest = panel.height;
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
					helper.list.Remove(bt);
					bt.Dispose();
				}
			};
			ShellManager.Current.WindowCreated += (wnd) => {
				if (wnd.ShowInTaskbar())
					createButton(wnd);
			};

			//box.SizeAllocated += (s, e) => 
			//	OnSizeAllocated(e);
		}

		private void OnSizeAllocated(SizeAllocatedArgs args)
		{

		}

		public void Update()
		{
			foreach (Widget child in helper.list) {
				helper.list.Remove(child);
				child.Dispose();
			}

			foreach (var wnd in ShellManager.Current.Windows) {
				if (wnd.ShowInTaskbar()) {
					createButton(wnd);
				}
			}

		}

		public class TWindowButton : PanelButton
		{

			public TWindow wnd;

			public TWindowButton(TWindow wnd)
			{
				this.wnd = wnd;
				Events = EventMask.AllEventsMask;
			}

			protected override void OnToggled()
			{
				base.OnToggled();
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

			var but = new TWindowButton(wnd);
			but.Add(b);
			helper.list.Add(but);

			var img = wnd.GetIcon(new Size(22, 22));
			if (img != null) {
				img.Events = EventMask.AllEventsMask;
				//but.Image = img;
				b.Add(img);
			}

			var lab = new Label();
			lab.Ellipsize = Pango.EllipsizeMode.End;
			lab.Text = wnd.GetName();
			lab.Events = EventMask.AllEventsMask;

			lab.WidthRequest = 100;

			b.Add(lab);

			b.ShowAll();

			buthash2.Add(wnd, but);

			but.Clicked += (s, e) => {
				//wnd.BringToFront();
			};

		}

		public override Widget CreateWidget()
		{
			return helper.GetRoot();
		}

	}

}

