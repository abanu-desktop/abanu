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

		#region IEnumerable implementation

		public IEnumerator<PanelButton> GetEnumerator()
		{
			return buttons.GetEnumerator();
		}

		private CustomBox rowBox;

		public class CustomBox : Box
		{
			public CustomBox(Orientation ori)
				: base(ori, 0)
			{
			}

			protected override void OnGetPreferredWidth(out int minimum_width, out int natural_width)
			{
				base.OnGetPreferredWidth(out minimum_width, out natural_width);
				minimum_width = 0;
				natural_width = 0;
			}

		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return buttons.GetEnumerator();
		}

		#endregion

		public PanelButtonContainer(Orientation ori)
		{
			rowBox = new CustomBox(ori == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical);
			for (var i = 0; i < rowsCount; i++) {
				var row = new PanelButtonList(ori);
				rows.Add(row);
				rowBox.Add(row);
			}

			rowBox.SizeAllocated += (s, e) => {
				alloc = e.Allocation;
			};


		}

		private Rectangle alloc;

		public void updateSize()
		{
		}

		public int rowHeight = -1;
		public int rowsCount = 1;
		private List<PanelButtonList> rows = new List<PanelButtonList>();

		public List<PanelButton> buttons = new List<PanelButton>();

		public void Add(PanelButton bt)
		{
			buttons.Add(bt);
			UpdateButtons(); //TODO: async queue
		}

		public void Remove(PanelButton bt)
		{
			buttons.Remove(bt);
			UpdateButtons();
		}

		private void UpdateButtons()
		{
			foreach (var row in rows) {
				foreach (PanelButton bt in row) {
					row.Remove(bt);
				}
			}

			foreach (var bt in buttons) {
				rows[0].Add(bt);
			}

		}

		public bool expand = true;

		public Widget GetRoot()
		{
/*			if (expand) {
				var box2 = new Layout(new Adjustment(0, 0, 0, 0, 0, 0), new Adjustment(0, 0, 0, 0, 0, 0));
				box2.Add(rowBox);
				return box2;
			} else {
				return rowBox;
			}*/

			return rowBox;
		}

	}

}

