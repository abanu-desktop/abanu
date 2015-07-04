using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

using abanu.core;

namespace abanu.panel
{

	public class TasksPlugin : TPlugin
	{

		private TMyButtonList box;
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
			box = new TMyButtonList(Orientation.Horizontal);
			box.HeightRequest = panel.height;
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

			box.SizeAllocated += (s, e) => 
				OnSizeAllocated(e);
		}

		private void OnSizeAllocated(SizeAllocatedArgs args)
		{

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

		public class TWindowButton : TMyButton
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

			var but = new  TWindowButton(wnd);

			var b = new HBox();

			var img = wnd.GetIcon(new Size(22, 22));
			if (img != null) {
				b.PackStart(img, true, true, 0);
			}

			var l = new Label(wnd.GetName());
			l.Ellipsize = Pango.EllipsizeMode.End;
			b.PackStart(l, true, true, 0);
			l.Justify = Justification.Left;

			//but.LabelWidget = b;
			but.Label = "dddd";


			//but.Label = "ssssssssssssssssssssssssssssss";
			//but.IconName = "search";
			//but.IconName = "search";
			//but.SetSizeRequest(100, 50);
			but.WidthRequest = 100;
			box.Add(but);

			//b.Add(lab);

			//but.SetSizeRequest(150, box.HeightRequest);

			b.ShowAll();

			buthash2.Add(wnd, but);

			but.Clicked += (s, e) => {
				//wnd.BringToFront();
			};

		}

		private TMenuButtonHelper helper;

		public override Widget CreateWidget()
		{
			return box;
		}

	}

}

