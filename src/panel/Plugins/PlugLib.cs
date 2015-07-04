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
		public TPanel panel;

		public TPlugin(TPanel panel)
		{
			this.panel = panel;
		}

	}

	public class cnt : Bin
	{
		

	}

	public class TMyButtonList :  Box
	{

		public TMyButtonList(Orientation ori)
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

	public class TMyButton : ToggleButton
	{
		
	}

}

