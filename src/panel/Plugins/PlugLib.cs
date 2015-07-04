using System;
using Gtk;
using Gdk;
using System.Collections.Generic;

using abanu.core;

namespace abanu.panel
{

	public abstract class TPlugin
	{

		public TPanel panel;
		public bool expand;

		public TPlugin(TPanel panel)
		{
			this.panel = panel;
		}

		public abstract Widget CreateWidget();

	}

	public class PanelButtonList :  Box
	{

		public PanelButtonList(Orientation ori)
			: base(ori, 0)
		{
			Orientation = ori;
			this.Margin = 0;
			this.BorderWidth = 0;
			//var css = new CssProvider();
			//css.LoadFromPath("../res/style/main.css");
			//StyleContext.AddProvider(css, uint.MaxValue);
			//this.ToolbarStyle = ToolbarStyle.BothHoriz;
		}

	}

	public class PanelButton : ToggleButton
	{
		
	}

	public class PanelButtonContainer
	{
		public PanelButtonContainer(Orientation ori)
		{
			list = new PanelButtonList(ori);
		}

		public PanelButtonList list;

		public void Add(PanelButton but)
		{
			list.Add(but);
		}

		public bool expand = true;

		public Widget GetRoot()
		{
			if (expand) {
				var box2 = new Layout(new Adjustment(0, 0, 0, 0, 0, 0), new Adjustment(0, 0, 0, 0, 0, 0));
				box2.Add(list);
				return box2;
			} else {
				return list;
			}
		}

	}

}

