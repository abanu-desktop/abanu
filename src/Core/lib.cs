using System;

namespace PanelShell
{
	public static class AppLib
	{

		public static void log(string text)
		{
			LogWindow.Current.AppendText(text);
		}


		public static void MessageBox(string text)
		{
			
		}
	}
}

