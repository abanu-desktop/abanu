using System;
using Microsoft.Win32;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using abanu.core;

namespace abanu.win32
{

	public class FactoryActivator : abanu.core.FactoryActivator
	{
		public override ShellManager CreateShellManager()
		{
			return new ShellManagerWin32();
		}

		public override TWindow CreateWindow(IntPtr handle)
		{
			return new TWindowWin32(handle);
		}

		public override TLauncherEntry ReadLinkFile(string file)
		{
			if (Path.GetExtension(file).ToLower() == ".desktop")
				return Factory.Unix.ReadLinkFile(file);

			return LinkReaderWin32.CreateFromFileLnk(file);
		}
	}

}

