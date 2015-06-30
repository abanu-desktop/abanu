using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

namespace PanelShell
{

	public class TPlugin
	{
		
		public Widget widget;

	}

	public class MenuPlugin : TPlugin
	{

		private Button button;

		public MenuPlugin()
		{
			widget = button = new Button();
			button.Add(new Label("Menu"));
			button.Clicked += (s, e) => {
				
			};
		}

	}

	public class TasksPlugin : TPlugin
	{

		private Box box;
		//private Dictionary<Button, TWindow> buthash=new Dictionary<Button, TWindow>();

		public TasksPlugin()
		{
			widget = box = new Box(Orientation.Horizontal, 0);
			Update();
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

		private void createButton(TWindow wnd)
		{

			var b = new HBox();
			b.Events = EventMask.AllEventsMask;

			var but = new Button();
			but.Add(b);
			box.Add(but);

			var ico = wnd.GetIcon();
			var pbuf = new Pixbuf(ico);
			var img = new Image(pbuf);
			img.Events = EventMask.AllEventsMask;
			//but.Image = img;
			b.Add(img);

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

			but.Clicked += (s, e) => {
				wnd.BringToFront();
			};

		}

	}

}

