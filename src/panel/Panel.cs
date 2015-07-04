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
			//Decorated = false;
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

		public TPanel()
		{
			win = new PanelWindow();
		}

		public void Configure()
		{
			SetOrientation(Orientation.Horizontal);
			SetPos(0, new Point(0, 0), 30, 1, 100.0, EDock.Top);

			TPlugin plug = new MenuPlugin(this);
			AddPlugin(plug);
			plug = new TasksPlugin(this);
			AddPlugin(plug, true);
			plug = new DatePlugin(this);
			AddPlugin(plug, false, true);
		}

		public void SetOrientation(Orientation ori)
		{
			box.Orientation = ori;
		}

		public int panelSize;

		public int monitorIdx;
		public int rowHeight;
		public int rows;


		public void SetPos(int monitorIdx, Point pos, int rowHeight, int rows, double widthPercent, EDock dock)
		{
			
			this.rows = rows;
			this.rowHeight = rowHeight;
			panelSize = rowHeight * rows;
			height = panelSize;
			var mon = Screen.Default.GetMonitorGeometry(monitorIdx);

			if (dock.HasFlag(EDock.Top))
				pos.Y = 0;
			if (dock.HasFlag(EDock.Bottom))
				pos.Y = mon.Height - panelSize;

			width = (int)(((double)mon.Width / 100) * widthPercent);

			win.Move(pos.X, pos.Y);
			win.SetDefaultSize(width, height);
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

		public void AddPlugin(TPlugin plug, bool expand = false, bool last = false)
		{
			Widget w;
			if (expand) {
				var box2 = new Layout(new Adjustment(0, 0, 0, 0, 0, 0), new Adjustment(0, 0, 0, 0, 0, 0));

				box2.Add(plug.CreateWidget());
				w = box2;
			} else {
				w = plug.CreateWidget();
			}

			plugins.Add(plug);
			if (last)
				box.PackEnd(w, expand, expand, 0);
			else
				box.PackStart(w, expand, expand, 0);
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

