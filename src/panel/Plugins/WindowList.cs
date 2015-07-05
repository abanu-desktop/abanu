using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

using abanu.core;

namespace abanu.panel
{

	[PluginAttribute("windowlist")]
	public class WindowListPlugin : TPlugin
	{

		private PanelButtonTable buttonTable;
		private Dictionary<TWindow, TWindowButton> buthash2 = new Dictionary<TWindow, TWindowButton>();

		public TWindowButton GetButton(TWindow wnd)
		{
			TWindowButton bt = null;
			buthash2.TryGetValue(wnd, out bt);
			return bt;
		}

		public WindowListPlugin(TPanel panel, PluginConfig cfg)
			: base(panel, cfg)
		{
			buttonTable = new PanelButtonTable(Orientation.Horizontal);
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
					buttonTable.Remove(bt);
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
			foreach (var child in buttonTable) {
				buttonTable.Remove(child);
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
			}

			/*
			protected override void OnToggled()
			{
				base.OnToggled();
			}
*/

			internal override bool OnButtonPressEvent(EventButton evnt, Func<EventButton, bool> callBase)
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

			var but = new TWindowButton(wnd);

			var img = wnd.GetIcon(new Size(22, 22));
			if (img != null) {
				but.Image = img;
				img.Show();
			}

			but.Label = wnd.GetName();
			buttonTable.Add(but);

			buthash2.Add(wnd, but);

			//but.Clicked += (s, e) => {
			//wnd.BringToFront();
			//};

		}

		public override Widget CreateWidget()
		{
			return buttonTable.GetRoot();
		}

	}

}

