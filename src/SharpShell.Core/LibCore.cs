using System;
using Gtk;

namespace SharpShell.Core
{
	public static class LibCore
	{
		
		public static void Log(string txt)
		{
		}

		public static void MessageBox(string text)
		{
			new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, text).Show();
		}

	}
}

