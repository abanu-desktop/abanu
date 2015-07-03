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

		public static class Factory
		{
			public static FactoryActivator Current;
			public static FactoryActivator Win32;
			public static FactoryActivator Unix;

			public static void Load()
			{

			}
		}

		public abstract class FactoryActivator
		{

			public void GetLauncherReader()
			{
			}

			public void GetShellManager()
			{
			}

		}

	}
}

