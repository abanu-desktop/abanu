using System;
using Gtk;
using Gdk;

using SharpShell.Core;

namespace SharpShell.Panel
{
	public class LogWindow : Gtk.Window
	{

		public static LogWindow Current;

		private TextBuffer buf;
		private ScrolledWindow sv;

		public LogWindow()
			: base(Gtk.WindowType.Toplevel)
		{
			Current = this;
			SetSizeRequest(800, 800);
			var tab = new TextTagTable();
			buf = new TextBuffer(tab);
			var en = new TextView(buf);
			sv = new ScrolledWindow();
			sv.Add(en);
			Add(sv);

			CoreLib.OnLog += (txt) => AppendText(txt);

			ShowAll();
		}

		public void AppendText(string text)
		{
			buf.Text += text + Environment.NewLine;
		}

	}
}

