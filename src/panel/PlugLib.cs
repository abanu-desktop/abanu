using System;
using Gtk;
using Gdk;
using System.Collections;
using System.Collections.Generic;

using abanu.core;

namespace abanu.panel
{

	public class PluginAttribute : Attribute
	{
		public string Name;

		public PluginAttribute(string name)
		{
			this.Name = name;
		}
	}

	public abstract class TPlugin
	{

		public TPanel panel;
		public bool expand;
		public PluginConfig cfg;

		public TPlugin(TPanel panel, PluginConfig cfg)
		{
			this.panel = panel;
			this.cfg = cfg;
			this.expand = cfg.Expand;
		}

		public abstract Widget CreateWidget();

		public static Type GetPluginType(string name)
		{
			foreach (var t in typeof(AppLib).Assembly.GetTypes()) {
				foreach (var attr in t.GetCustomAttributes(typeof(PluginAttribute), false)) {
					if (((PluginAttribute)attr).Name == name)
						return t;
				}
			}
			return null;
		}

		public static IEnumerable<Type> GetPluginTypes()
		{
			foreach (var t in typeof(AppLib).Assembly.GetTypes()) {
				foreach (var attr in t.GetCustomAttributes(typeof(PluginAttribute), false)) {
					yield return t;
					continue;
				}
			}
		}

		public static TPlugin CreateIntance(TPanel panel, PluginConfig cfg)
		{
			var t = GetPluginType(cfg.Type);
			return (TPlugin)Activator.CreateInstance(t, new object[]{ panel, cfg });
		}

	}

	public class PanelButton : IDisposable
	{

		public PanelButton()
		{
			button = new PanelButtonWidget(this);
			button.Events = EventMask.AllEventsMask;
			//button.WidthRequest = 100;

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

		internal virtual bool OnButtonPressEvent(EventButton evnt, Func<EventButton, bool> callBase)
		{
			return callBase(evnt);
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
		private Label label;
		private Image image;
		private HBox box;

	}

	public class PanelButtonWidget : ToggleButton
	{

		public PanelButton panelButton;

		public PanelButtonWidget(PanelButton panelButton)
		{
			this.panelButton = panelButton;
		}

		protected override bool OnButtonPressEvent(EventButton evnt)
		{
			return panelButton.OnButtonPressEvent(evnt, (arg) => {
				return base.OnButtonPressEvent(arg);
			});
		}

	}

	public class PanelButtonTable : IEnumerable<PanelButton>
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

		private Table rowBox;

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return buttons.GetEnumerator();
		}

		#endregion

		public PanelButtonTable(Orientation ori)
		{
			rowBox = new Table(0, 0, false);
			rowBox.Valign = Align.Start;

			CoreLib.OnSignal += (path, args) => {
				UpdateButtons();
			};

			Rectangle alloc = new Rectangle();
			rowBox.SizeAllocated += (s, e) => {
				if (alloc.Width != e.Allocation.Width) {
					alloc = e.Allocation;

					Application.Invoke((ss, ee) => {
						UpdateButtons();
					});

				}
			};
		}

		//public int rowHeight = 30;
		public int rowsCount = 3;

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

		private uint buttonWidthNormal = 100;

		private void UpdateButtons()
		{
			if (rowBox.Parent == null)
				return;

			//var height = rowHeight * rowsCount;
			var width = rowBox.AllocatedWidth;

			//rowBox.HeightRequest = 30;

			foreach (PanelButton bt in buttons) {
				if (bt.button.Parent != null)
					rowBox.Remove(bt.button);
			}

			var numNormal = width / buttonWidthNormal;

			rowBox.Resize((uint)numNormal, (uint)rowsCount);

			uint currCount = 0;
			uint currRow = 0;
			foreach (var bt in buttons) {
				if (currCount >= numNormal) {
					currRow++;
					currCount = 0;
				}
				rowBox.Attach(bt.button, currCount + 0, currCount + 1, currRow, currRow + 1);
				currCount++;
			}
		}

		public Widget GetRoot()
		{
			return rowBox;
		}

	}

}

