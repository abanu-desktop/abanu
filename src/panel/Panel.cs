using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

namespace abanu.panel
{
	
	public class PanelWindow : Gtk.Window
	{
		
		public PanelWindow()
			: base(Gtk.WindowType.Toplevel)
		{
			KeepAbove = true;
			//this.Resizable = false;
			//this.SetDefaultSize(300, 300);
			//this.SetGeometryHints(this, new Geometry(){ MaxWidth = 300 }, WindowHints.MaxSize);
		}

	}

	public class TPanel
	{

		private PanelWindow win;
		private Box box;
		private Menu menu;

		public PanelConfig cfg;

		public TPanel(PanelConfig cfg)
		{
			this.cfg = cfg;
			win = new PanelWindow();
		}

		public void Configure()
		{
			SetOrientation(cfg.Orientation);
			SetPos(cfg.Monitor, new Point(cfg.X, cfg.Y), cfg.RowHeight, cfg.Rows, cfg.Size, cfg.Dock);

			foreach (var plugCfg in cfg.Plugins) {
				var p = TPlugin.CreateIntance(this, plugCfg);
				AddPlugin(p);
			}
		}

		public void SetOrientation(Orientation ori)
		{
			box.Orientation = ori;
		}

		public int panelSize;

		public int monitorIdx;
		public int rowHeight;
		public int rows;


		public void SetPos(int monitorIdx, Point pos, int rowHeight, int rows, string size, EDock dock)
		{
			var mon = Screen.Default.GetMonitorGeometry(monitorIdx);

			if (size.EndsWith("%")) {
				var widthPercent = double.Parse(size.Substring(0, size.Length - 1), System.Globalization.CultureInfo.InvariantCulture);
				width = (int)(((double)mon.Width / 100) * widthPercent);
			} else {
				width = int.Parse(size);	
			}

			this.rows = rows;
			this.rowHeight = rowHeight;
			panelSize = rowHeight * rows;
			height = panelSize;

			if (dock.HasFlag(EDock.Top))
				pos.Y = 0;
			if (dock.HasFlag(EDock.Bottom))
				pos.Y = mon.Height - panelSize;

			win.Move(pos.X, pos.Y);
			win.SetDefaultSize(width, height);
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

			//win.Add(box2);

			//box2.SetSizeRequest(300, 300);


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
			var w = plug.CreateWidget();

			plugins.Add(plug);
			box.PackStart(w, plug.expand, plug.expand, 0);
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

