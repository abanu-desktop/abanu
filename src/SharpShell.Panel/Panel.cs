using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

namespace SharpShell.Panel
{
	
	public class PanelWindow : Gtk.Window
	{
		
		public PanelWindow()
			: base(Gtk.WindowType.Popup)
		{
			Decorated = false;
		}

	}

	public class TPanel
	{

		private PanelWindow win;
		private Box box;
		private Menu menu;

		public TPanel()
		{
			win = new PanelWindow();
		}

		public void Configure()
		{
			SetOrientation(Orientation.Horizontal);
			SetPos(0, new Point(0, 0), 30, 1, 100.0, EDock.Top);

			TPlugin plug = new MenuPlugin();
			AddPlugin(plug);
			plug = new TasksPlugin();
			AddPlugin(plug);
		}

		public void SetOrientation(Orientation ori)
		{
			box.Orientation = ori;
		}

		public int panelSize;

		public int monitorIdx;

		public void SetPos(int monitorIdx, Point pos, int rowHeight, int rows, double widthPercent, EDock dock)
		{

			panelSize = rowHeight * rows;
			height = panelSize;
			var mon = Screen.Default.GetMonitorGeometry(monitorIdx);

			if (dock.HasFlag(EDock.Top))
				pos.Y = 0;
			if (dock.HasFlag(EDock.Bottom))
				pos.Y = mon.Height - panelSize;

			width = (int)(((double)mon.Width / 100) * widthPercent);

			win.Move(pos.X, pos.Y);
			win.SetSizeRequest(width, height);
		}

		public int width;
		public int height;

		public void Setup()
		{
			menu = new Menu();
			var quitItem = new MenuItem("Quit");
			menu.Add(quitItem);
			quitItem.ButtonPressEvent += (s, e) => {
				Application.Quit();
			};

			box = new Box(Orientation.Vertical, 1);
			win.Add(box);

			Configure();

			win.Events = EventMask.AllEventsMask;
			//box.Events = EventMask.AllEventsMask;

			win.ButtonPressEvent += (s, e) => {
				if (e.Event.Button == 3) {
					menu.Popup();
				}
			};
			menu.ShowAll();
		}

		public void Show()
		{
			win.ShowAll();
		}

		public void AddPlugin(TPlugin plug)
		{
			plugins.Add(plug);
			box.Add(plug.widget);
		}

		public TPluginList plugins = new TPluginList();

	}

	public class TPluginList  : List<TPlugin>
	{

	}

	public enum EDock
	{
		Top,
		Bottom,
		Left,
		Right
	}
}

