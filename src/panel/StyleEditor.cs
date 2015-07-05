using System;
using Gtk;
using Gdk;

using abanu.core;

namespace abanu.panel
{
	public class StyleWindow : Gtk.Window
	{

		private TextBuffer buf;
		private ScrolledWindow sv;

		public StyleWindow()
			: base(Gtk.WindowType.Toplevel)
		{
			SetSizeRequest(800, 800);

			var box = new VBox();
			Add(box);

			var tab = new TextTagTable();
			buf = new TextBuffer(tab);
			buf.Text = System.IO.File.ReadAllText("res/theme/gtk.css");
			var en = new TextView(buf);
			sv = new ScrolledWindow();
			sv.Add(en);
			box.PackStart(sv, true, true, 0);

			var cssProvider = new CssProvider();
			StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, uint.MaxValue - 10);

			var isDefault = true;

			var but = new Button();
			but.Label = "Save";
			but.HeightRequest = 30;
			box.PackEnd(but, false, false, 0);
			but.Clicked += (s, e) => {
				System.IO.File.WriteAllText("res/theme/gtk.css", buf.Text);
			};

			buf.Changed += (s, e) => {
				bool error = false;
				try {
					//StyleContext.RemoveProviderForScreen(Gdk.Screen.Default, cssProvider);
					cssProvider.LoadFromData(buf.Text);
					//StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, uint.MaxValue - 10);		
				} catch (Exception ex) {
					error = true;
				}
				if (error) {
					if (!isDefault) {
						StyleContext.RemoveProviderForScreen(Gdk.Screen.Default, cssProvider);
						StyleContext.AddProviderForScreen(Gdk.Screen.Default, AppLib.GlobalCssProvider, uint.MaxValue);		
						isDefault = true;
					}

				} else {
					if (isDefault) {
						StyleContext.RemoveProviderForScreen(Gdk.Screen.Default, AppLib.GlobalCssProvider);
						StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, uint.MaxValue);		
						isDefault = false;
					}
				}

			};

			ShowAll();
		}

	}
}

