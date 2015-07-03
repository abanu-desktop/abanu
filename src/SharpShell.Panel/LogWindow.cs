using System;
using Gtk;
using Gdk;

namespace SharpShell.Panel
{
	public class LogWindow : Gtk.Window
	{

		public static LogWindow Current;

		private TextBuffer buf;

		public LogWindow()
			: base(Gtk.WindowType.Toplevel)
		{
			Current = this;
			SetSizeRequest(800, 800);
			var tab = new TextTagTable();
			buf = new TextBuffer(tab);
			var en = new TextView(buf);
			var sv = new ScrolledWindow();
			sv.Add(en);
			Add(sv);

			ShowAll();
		}

		public void AppendText(string text)
		{
			buf.Text += text + Environment.NewLine;
		}

	}
}

