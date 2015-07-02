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

namespace PanelShell
{
	public class LauncherWidget : HPaned
	{
		
		public LauncherWidget()
		{
			/*var tab = new TextTagTable();
			var buf = new TextBuffer(tab);
			var tb = new TextView(buf);
			this.Add(tb);
			tb.HeightRequest = 20;
*/



			/*Add(CreateList());
			Add(CreateList());*/


			Add1(CreateAppList());
			Add2(CreateCatList());

			ShowAllApps();

			ShowAll();
		}

		private Widget CreateAppList()
		{
			var scroll = new ScrolledWindow();

			appListStore = new ListStore(typeof(string));

			var tv = new TreeView(appListStore);
			scroll.Add(tv);

			var cellRender = new CellRendererText();
			var col = new TreeViewColumn();
			col.Title = "Name";
			col.PackStart(cellRender, true);
			col.AddAttribute(cellRender, "markup", 0);
			tv.AppendColumn(col);

			var frame = new Frame();
			frame.Add(scroll);
			frame.SetSizeRequest(200, 200);

			return frame;
		}

		private ListStore appListStore;

		public void ShowCategory(TLauncherCategory entry)
		{
			ShowApps(TLauncherIndex.Current.ByCategory(entry));
		}

		public void ShowApps(IEnumerable<TLauncherEntry> items)
		{
			appListStore.Clear();
			foreach (var entry in items) {
				appListStore.AppendValues(entry.Name);
			}
		}

		public void ShowAllApps()
		{
			ShowApps(TLauncherIndex.Current.All());
		}

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

			foreach (var entry in TLauncherIndex.Current.Categories) {
				var bt = createCatButton(entry);
				//box.PackStart(bt, false, false, 2);
				tb.Add(bt);
			}

			var frame = new Frame();
			frame.Add(scroll);
			return frame;
		}

		private List<ToggleButton2> catButtons = new List<ToggleButton2>();

		private bool inToggle = false;

		public Widget createCatButton(TLauncherCategory entry)
		{
			
			var bt = new  ToggleButton2("");

			//bt.Label = entry.Name;
			var lab = new Label(entry.Name);
			bt.LabelWidget = lab;
			//lab.SetAlignment(0f, 0f);
			//lab.Justify = Justification.Left;

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

