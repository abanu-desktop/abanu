using System;
using Gtk;
using Gdk;
using System.Collections;
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

	public class PanelButton : ToggleButton
	{
		
	}

	public class PanelButtonContainer : IEnumerable<PanelButton>
	{

		public class PanelButtonList :  Box
		{

			public PanelButtonList(Orientation ori)
				: base(ori, 0)
			{
			}

		}

		public class Row :  Box
		{

			public PanelButtonList buttonBox;

			public Row(Orientation ori)
				: base(ori, 0)
			{
				buttonBox = new PanelButtonList(ori);
			}

		}

		#region IEnumerable implementation

		public IEnumerator<PanelButton> GetEnumerator()
		{
			return buttons.GetEnumerator();
		}

		private Box rowBox;

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return buttons.GetEnumerator();
		}

		#endregion

		public PanelButtonContainer(Orientation ori)
		{
			buttonBox = new PanelButtonList(ori);
			rowBox = new Row(ori);
		}

		public int rowHeight = -1;
		public int rows = 1;

		private PanelButtonList buttonBox;

		public List<PanelButton> buttons = new List<PanelButton>();

		public void Add(PanelButton bt)
		{
			buttons.Add(bt);
			buttonBox.Add(bt);
			UpdateButtons(); //TODO: async queue
		}

		public void Remove(PanelButton bt)
		{
			buttons.Remove(bt);
			buttonBox.Add(bt);
			UpdateButtons();
		}

		private void UpdateButtons()
		{
			
		}

		public bool expand = true;

		public Widget GetRoot()
		{
			if (expand) {
				var box2 = new Layout(new Adjustment(0, 0, 0, 0, 0, 0), new Adjustment(0, 0, 0, 0, 0, 0));
				box2.Add(buttonBox);
				return box2;
			} else {
				return buttonBox;
			}
		}

	}

}

