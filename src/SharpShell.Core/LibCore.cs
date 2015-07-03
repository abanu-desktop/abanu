using System;
using Gtk;

namespace SharpShell.Core
{
	public static class CoreLib
	{
		
		public static void Log(string txt)
		{
			if (OnLog != null)
				OnLog(txt);
		}

		public static event Action<string> OnLog;

		public static void MessageBox(string text)
		{
			new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, text).Show();
		}

		public static bool IsWin32 {
			get { 
				return Environment.OSVersion.Platform != PlatformID.Unix;
			}
		}

		public static bool IsUnix {
			get { 
				return Environment.OSVersion.Platform == PlatformID.Unix;
			}
		}

	}

	public static class Factory
	{
		public static FactoryActivator Current;
		public static FactoryActivator Win32;
		public static FactoryActivator Unix;

		static Factory()
		{
			Win32 = Load("Win32");
			Unix = Load("Unix");

			if (CoreLib.IsWin32)
				Current = Win32;
			else
				Current = Unix;
		}

		private static FactoryActivator Load(string platform)
		{
			var asm = System.Reflection.Assembly.LoadFrom("SharpShell." + platform + ".dll");
			var t = asm.GetType("SharpShell." + platform + ".FactoryActivator");
			return (FactoryActivator)Activator.CreateInstance(t);
		}

	}

	public abstract class FactoryActivator
	{

		public abstract TWindow CreateWindow(IntPtr handle);

		public virtual void GetLauncherReader()
		{
			throw new NotImplementedException();
		}

		public virtual ShellManager CreateShellManager()
		{
			throw new NotImplementedException();
		}

		public abstract TLauncherEntry ReadLinkFile(string file);

	}

}

