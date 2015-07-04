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

	public class PanelButton : IDisposable
	{

		public PanelButton()
		{
			/*box = new Alignment();
			box.WidthRequest = 100;
			box.Show();
*/



			button = new PanelButtonWidget();
			button.Events = EventMask.AllEventsMask;
			button.WidthRequest = 100;

			box = new HBox();
			button.Add(box);

			label = new Gtk.Label();
			label.Ellipsize = Pango.EllipsizeMode.End;
			label.Halign = Align.Start;
			box.PackEnd(label, true, true, 0);

			button.Show();
			label.Show();
			box.Show();
		}

		#region IDisposable implementation

		public void Dispose()
		{
			//box.Dispose();
			button.Dispose();
			//box = null;
			button = null;
			label.Dispose();
			label = null;

			box.Dispose();
			box = null;
		}

		#endregion

		public Image ico;

		public string Label {
			get {
				return label.Text;
			} 
			set { 
				label.Text = value;
			}
		}

		public bool Active { 
			get { 
				return button.Active;
			} 
			set {
				button.Active = value;
			}
		}

		public Image Image {
			get { 
				return image;
			}
			set {
				if (image == value)
					return;
				if (image != null)
					box.Remove(image);

				if (value != null) {
					box.PackStart(value, false, false, 0);
					value.MarginRight = 4;
				}
				image = value;
			}
		}

		public PanelButtonWidget button;
		public Label label;
		private Image image;
		private HBox box;
		//public Alignment box;
	}

	public class PanelButtonWidget : ToggleButton
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

		//private CustomBox rowBox;
		private Table rowBox;

		public class CustomBox : Box
		{
			public CustomBox(Orientation ori)
				: base(ori, 0)
			{
			}

			/*			protected override void OnGetPreferredWidth(out int minimum_width, out int natural_width)
			{
				//base.OnGetPreferredWidth(out minimum_width, out natural_width);
				minimum_width = 0;
				natural_width = 0;
			}*/

		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return buttons.GetEnumerator();
		}

		#endregion

		public PanelButtonContainer(Orientation ori)
		{
			//rowBox = new CustomBox(ori == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical);
			rowBox = new Table(1, 18, false);
			for (var i = 0; i < rowsCount; i++) {
				/*var row = new PanelButtonList(ori);
				rows.Add(row);
				rowBox.Add(row);*/
			}

			CoreLib.OnSignal += (path, args) => {
				UpdateButtons();
			};

			rowBox.SizeAllocated += (s, e) => {
				//alloc = e.Allocation;
				Application.Invoke((a, b) => {
					//UpdateButtons();
				});
				//UpdateButtons();
			};


		}

		//private Rectangle alloc;

		public int rowHeight = -1;
		public int rowsCount = 1;
		//private List<PanelButtonList> rows = new List<PanelButtonList>();

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

		private int buttonWidthNormal = 100;

		private void UpdateButtons()
		{
			var width = rowBox.AllocatedWidth;

			foreach (PanelButton bt in buttons) {
				if (bt.button.Parent != null)
					rowBox.Remove(bt.button);
			}

			var numNormal = width / buttonWidthNormal;
			numNormal = 18;

			uint currCount = 0;
			foreach (var bt in buttons) {
				if (currCount < numNormal) {
					//bt.Show();
					//bt.WidthRequest = buttonWidthNormal;
					//bt.Expand = false;
					//rows[0].Add(bt.button);
					if (currCount < 18)
						rowBox.Attach(bt.button, currCount + 0, currCount + 1, 0, 1);

					currCount++;
				}
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

