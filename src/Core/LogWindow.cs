using System;
using Gtk;
using Gdk;

namespace PanelShell
{
	public class LogWindow : Gtk.Window
	{

		public static LogWindow Current;

		private TextBuffer buf;

		public LogWindow()
			: base(Gtk.WindowType.Toplevel)
		{
			Current = this;
			var tab = new TextTagTable();
			buf = new TextBuffer(tab);
			var en = new TextView(buf);
			Add(en);

			ShowAll();
		}

		public void AppendText(string text)
		{
			buf.Text += text + Environment.NewLine;
		}

	}
}

