using System;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Search;
using System.IO;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Gtk;
using Gdk;

using SharpShell.Core;

namespace SharpShell.Panel
{
	public class LauncherWidget : VBox
	{

		private HPaned hpaned;

		public LauncherWidget()
		{
			var tb = new Entry("");
			tb.SetIconFromIconName(EntryIconPosition.Secondary, "search");
			tb.Changed += (s, e) => {
				ShowSearch(tb.Text);
			};

			tb.KeyReleaseEvent += (s, e) => {
				if (e.Event.Key == Gdk.Key.Return && appListStore.NColumns != 0) { //TODO: appListStore.NColumns does not work
					hpaned.Child1.ChildFocus(DirectionType.TabForward);
				}
				if (e.Event.Key == Gdk.Key.Escape) {
					Parent.Hide(); //TODO: detect Window
				}
			};

			tb.Margin = 5;
			//tb.BorderWidth = 1;
			PackStart(tb, false, false, 0);

			hpaned = new HPaned();

			Add(hpaned);

			/*Add(CreateList());
			Add(CreateList());*/


			var appList = CreateAppList();
			hpaned.Add1(appList);
			hpaned.Add2(CreateCatList());

			ShowAllApps();

			ShowAll();
			allButton.Active = true;
			lastActiveButton = allButton;
		}

		private Widget CreateAppList()
		{
			var scroll = new ScrolledWindow();

			appListStore = new ListStore(typeof(string), typeof(Pixbuf));

			appTv = new TreeView();
			scroll.Add(appTv);
			appTv.HeadersVisible = false;

			var col = new TreeViewColumn();
			col.Title = "Name";

			var colRender2 = new CellRendererPixbuf();
			col.PackStart(colRender2, false);
			col.AddAttribute(colRender2, "pixbuf", 1);

			var colRender = new CellRendererText();
			colRender.Ellipsize = Pango.EllipsizeMode.End;
			col.PackStart(colRender, true);
			col.AddAttribute(colRender, "markup", 0);

			appTv.AppendColumn(col);

			var frame = new Frame();
			frame.Add(scroll);
			frame.SetSizeRequest(300, 200);

			return frame;
		}

		private ListStore appListStore;
		private TreeView appTv;

		public void ShowSearch(string txt)
		{
			if (!string.IsNullOrEmpty(txt)) {
				ShowApps(TLauncherIndex.Current.BySearch(txt));	
				hpaned.Child2.Hide();
				hpaned.HandleWindow.Hide();
			} else {
				hpaned.Child2.Show();
				hpaned.HandleWindow.Show();
				ShowCategory(lastActiveCat);
			}
		}

		private TLauncherCategory lastActiveCat;

		public void ShowCategory(TLauncherCategory entry)
		{
			lastActiveCat = entry;
			ShowApps(TLauncherIndex.Current.ByCategory(entry));
		}

		public void ShowApps(IEnumerable<TLauncherEntry> items)
		{
			appTv.Model = null; //performance
			appListStore.Clear();
			foreach (var entry in items) {
				var markup = "<b>" + entry.Name + "</b>\n" + entry.Description;
				appListStore.AppendValues(markup, entry.GetIconPixBuf());
			}
			appTv.Model = appListStore;
		}

		public void ShowAllApps()
		{
			ShowCategory(TLauncherIndex.Current.catHash["All"]);
		}

		private ToggleButton2 allButton;
		private ToggleButton2 lastActiveButton;

		private Widget CreateCatList()
		{
			var scroll = new ScrolledWindow();
			var box = new VBox();
			scroll.Add(box);

			var tb = new Toolbar();

			tb.Orientation = Orientation.Vertical;
			tb.ToolbarStyle = ToolbarStyle.BothHoriz;
			tb.ShowArrow = false;

			box.Add(tb);

			allButton = createCatButton(TLauncherIndex.Current.catHash["All"]);
			tb.Add(allButton);

			tb.Add(new SeparatorToolItem());

			foreach (var entry in TLauncherIndex.Current.Categories)
				if (!entry.meta)
					tb.Add(createCatButton(entry));

			var noneButton = createCatButton(TLauncherIndex.Current.catHash["None"]);
			tb.Add(noneButton);

			var frame = new Frame();
			frame.Add(scroll);
			return frame;
		}

		private List<ToggleButton2> catButtons = new List<ToggleButton2>();

		private bool inToggle = false;

		private ToggleButton2 createCatButton(TLauncherCategory entry)
		{
			
			var bt = new  ToggleButton2("");

			var b = new HBox();
			var l = new Label(entry.Name);
			b.PackStart(l, false, false, 0);
			l.Justify = Justification.Left;

			bt.LabelWidget = b;

			if (entry.HasIcon) {
				if (Environment.OSVersion.Platform == PlatformID.Unix)
					bt.IconName = entry.IconName;
				//bt.IconWidget = new Image();

			}

/*			bt.Mode = true;
			bt.HeightRequest = 40;
			bt.Entered += (s, e) => {
				if (!bt.Active)
					bt.Mode = false;
			};
			bt.LeaveNotifyEvent += (s, e) => {
				if (!bt.Active)
					bt.Mode = true;
			};*/
			bt.Clicked += (s, e) => {
				//return;
			};


			bt.Toggled += (s, e) => {
				if (inToggle)
					return;
				else
					inToggle = true;

				try {
					foreach (var catButton in catButtons) {
						if (catButton != bt)
							catButton.Active = false;
					}
					bt.Active = true;

					ShowCategory(entry);
					lastActiveButton = bt;

				} finally {
					inToggle = false;
				}
			};

			//bt.MarginLeft = 2;
			//bt.MarginRight = 2;

			bt.Margin = 1;

			catButtons.Add(bt);
			return bt;
		}

		private class ToggleButton2 : ToggleToolButton
		{

			public ToggleButton2(string label)
				: base(label)
			{
			


			}

			/*protected override bool OnButtonPressEvent(EventButton evnt)
			{
				return false;
			}*/

		}

	}
}

