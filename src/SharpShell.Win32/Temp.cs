using System;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SharpShell.Core
{
	class GetLocalizedName
	{
		[DllImport("shell32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
		internal static extern int SHGetLocalizedName(string pszPath, StringBuilder pszResModule, ref int cch, out int pidsRes);

		[DllImport("user32.dll", EntryPoint = "LoadStringW", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
		internal static extern int LoadString(IntPtr hModule, int resourceID, StringBuilder resourceValue, int len);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadLibraryExW")]
		internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

		internal const uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;
		internal const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;

		[DllImport("kernel32.dll", ExactSpelling = true)]
		internal static extern int FreeLibrary(IntPtr hModule);

		[DllImport("kernel32.dll", EntryPoint = "ExpandEnvironmentStringsW", CharSet = CharSet.Unicode, ExactSpelling = true)]
		internal static extern uint ExpandEnvironmentStrings(string lpSrc, StringBuilder lpDst, int nSize);

		public static string GetName(string path)
		{
			StringBuilder sb = new StringBuilder(500);
			int len, id;
			len = sb.Capacity;

			if (SHGetLocalizedName(path, sb, ref len, out id) == 0) {
				ExpandEnvironmentStrings(sb.ToString(), sb, sb.Capacity);
				IntPtr hMod = LoadLibraryEx(sb.ToString(), IntPtr.Zero, DONT_RESOLVE_DLL_REFERENCES | LOAD_LIBRARY_AS_DATAFILE);
				if (hMod != IntPtr.Zero) {
					try {
						if (LoadString(hMod, id, sb, sb.Capacity) != 0) {
							return sb.ToString();
						}
					} finally {
						FreeLibrary(hMod);
					}
				}
			}
			return System.IO.Path.GetFileNameWithoutExtension(path);
		}


	}
}