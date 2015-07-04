// ***********************************************************************
//  LocalWindowsHook class
//  Dino Esposito, summer 2002 
// 
//  Provide a general infrastructure for using Win32 
//  hooks in .NET applications
// 
// ***********************************************************************

//
// I took this class from the example at http://msdn.microsoft.com/msdnmag/issues/02/10/cuttingedge
// and made a couple of minor tweaks to it - dpk
//

using System;
using System.Runtime.InteropServices;
using HWND = System.IntPtr;

namespace abanu.win32
{
	#region Class HookEventArgs
	public class HookEventArgs : EventArgs
	{
		public int HookCode;
		// Hook code
		public IntPtr wParam;
		// WPARAM argument
		public IntPtr lParam;
		// LPARAM argument
	}
	#endregion

	#region Enum HookType
	// Hook Types
	public enum HookType : int
	{
		WH_JOURNALRECORD = 0,
		WH_JOURNALPLAYBACK = 1,
		WH_KEYBOARD = 2,
		WH_GETMESSAGE = 3,
		WH_CALLWNDPROC = 4,
		WH_CBT = 5,
		WH_SYSMSGFILTER = 6,
		WH_MOUSE = 7,
		WH_HARDWARE = 8,
		WH_DEBUG = 9,
		WH_SHELL = 10,
		WH_FOREGROUNDIDLE = 11,
		WH_CALLWNDPROCRET = 12,
		WH_KEYBOARD_LL = 13,
		WH_MOUSE_LL = 14
	}
	#endregion

	#region Class LocalWindowsHook
	public class LocalWindowsHook
	{
		// ************************************************************************
		// Filter function delegate
		public delegate int HookProc(int code,IntPtr wParam,IntPtr lParam);
		// ************************************************************************

		// ************************************************************************
		// Internal properties
		protected IntPtr m_hhook = IntPtr.Zero;
		protected HookProc m_filterFunc = null;
		protected HookType m_hookType;
		// ************************************************************************

		// ************************************************************************
		// Event delegate
		public delegate void HookEventHandler(object sender,HookEventArgs e);
		// ************************************************************************

		// ************************************************************************
		// Event: HookInvoked
		public event HookEventHandler HookInvoked;

		protected void OnHookInvoked(HookEventArgs e)
		{
			if (HookInvoked != null)
				HookInvoked(this, e);
		}
		// ************************************************************************

		// ************************************************************************
		// Class constructor(s)
		public LocalWindowsHook(HookType hook)
		{
			m_hookType = hook;
			m_filterFunc = new HookProc(this.CoreHookProc); 
		}

		public LocalWindowsHook(HookType hook, HookProc func)
		{
			m_hookType = hook;
			m_filterFunc = func; 
		}
		// ************************************************************************

		// ************************************************************************
		// Default filter function
		protected int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
		{
			if (code < 0)
				return CallNextHookEx(m_hhook, code, wParam, lParam);

			// Let clients determine what to do
			HookEventArgs e = new HookEventArgs();
			e.HookCode = code;
			e.wParam = wParam;
			e.lParam = lParam;
			OnHookInvoked(e);

			// Yield to the next hook in the chain
			return CallNextHookEx(m_hhook, code, wParam, lParam);
		}
		// ************************************************************************

		// ************************************************************************
		// Install the hook
		public void Install()
		{
			m_hhook = SetWindowsHookEx(
				m_hookType, 
				m_filterFunc, 
				IntPtr.Zero, 
				(int)AppDomain.GetCurrentThreadId());
		}
		// ************************************************************************

		// ************************************************************************
		// Uninstall the hook
		public void Uninstall()
		{
			UnhookWindowsHookEx(m_hhook); 
			m_hhook = IntPtr.Zero;
		}
		// ************************************************************************

		public bool IsInstalled {
			get{ return m_hhook != IntPtr.Zero; }
		}

		#region Win32 Imports

		// ************************************************************************
		// Win32: SetWindowsHookEx()
		[DllImport("user32.dll")]
		protected static extern IntPtr SetWindowsHookEx(HookType code, 
		                                                HookProc func,
		                                                IntPtr hInstance,
		                                                int threadID);
		// ************************************************************************

		// ************************************************************************
		// Win32: UnhookWindowsHookEx()
		[DllImport("user32.dll")]
		protected static extern int UnhookWindowsHookEx(IntPtr hhook);
		// ************************************************************************

		// ************************************************************************
		// Win32: CallNextHookEx()
		[DllImport("user32.dll")]
		protected static extern int CallNextHookEx(IntPtr hhook, 
		                                           int code, IntPtr wParam, IntPtr lParam);
		// ************************************************************************

		#endregion
	}
	#endregion

	public class ShellHook_ : LocalWindowsHook
	{
		public ShellHook_()
			: base(HookType.WH_SHELL)
		{

			m_filterFunc = filterFunc;
		}

		private  int filterFunc(int code, IntPtr wParam, IntPtr lParam)
		{

			if (code < 0)
				return CallNextHookEx(m_hhook, code, wParam, lParam);

			// we're gonna ignore peek messages
//			if ( code == Win32.HC_ACTION )
//				RaiseMouseHookEvent( wParam, CrackHookMsg( wParam, (Win32.MOUSEHOOKSTRUCT)Marshal.PtrToStructure( lParam, typeof( Win32.MOUSEHOOKSTRUCT ) ) ) );

			// Yield to the next hook in the chain
			return CallNextHookEx(m_hhook, code, wParam, lParam);
		}

	}

	internal class Win32
	{
		public const int HC_ACTION = 0;
		public const int HC_NOREMOVE = 3;
	}

	public static class Interop
	{
		public enum ShellEvents : int
		{
			HSHELL_WINDOWCREATED = 1,
			HSHELL_WINDOWDESTROYED = 2,
			HSHELL_ACTIVATESHELLWINDOW = 3,
			HSHELL_WINDOWACTIVATED = 4,
			HSHELL_GETMINRECT = 5,
			HSHELL_REDRAW = 6,
			HSHELL_TASKMAN = 7,
			HSHELL_LANGUAGE = 8,
			HSHELL_ACCESSIBILITYSTATE = 11,
			HSHELL_APPCOMMAND = 12
		}

		[DllImport("user32.dll", EntryPoint = "RegisterWindowMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int RegisterWindowMessage(string lpString);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int DeregisterShellHookWindow(IntPtr hWnd);

		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int RegisterShellHookWindow(IntPtr hWnd);

		[DllImport("user32", EntryPoint = "GetWindowTextA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int GetWindowText(IntPtr hwnd, System.Text.StringBuilder lpString, int cch);

		[DllImport("user32", EntryPoint = "GetWindowTextLengthA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern int GetWindowTextLength(IntPtr hwnd);

		[DllImport("user32")]
		public static extern bool EnumWindows(WNDENUMPROC func, IntPtr lParam);

		public delegate bool WNDENUMPROC(HWND hwnd,IntPtr lParam);

		[DllImport("user32")]
		public static extern bool IsWindowVisible(HWND hWnd);

		[DllImport("user32")]
		public static extern HWND GetAncestor(HWND hWnd, uint gaFlags);

		[DllImport("user32")]
		public static extern HWND GetLastActivePopup(HWND hWnd);

		public static uint GA_PARENT = 1;
		public static uint GA_ROOT = 2;
		public static uint GA_ROOTOWNER = 3;

		[DllImport("user32")]
		public static extern bool GetTitleBarInfo(HWND hWnd, ref TITLEBARINFO pti);

		[StructLayout(LayoutKind.Sequential)]
		public struct TITLEBARINFO
		{
			public const int CCHILDREN_TITLEBAR = 5;
			public uint cbSize;
			public RECT rcTitleBar;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = CCHILDREN_TITLEBAR + 1)]
			public uint[] rgstate;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left, Top, Right, Bottom;
		}

		public static uint STATE_SYSTEM_FOCUSABLE = 0x00100000;
		public static uint STATE_SYSTEM_INVISIBLE = 0x00008000;
		public static uint STATE_SYSTEM_OFFSCREEN = 0x00010000;
		public static uint STATE_SYSTEM_UNAVAILABLE = 0x00000001;
		public static uint STATE_SYSTEM_PRESSED = 0x00000008;

		[DllImport("user32.dll", SetLastError = true)]
		public static extern long GetWindowLong(IntPtr hWnd, int nIndex);

		public const int GWL_EXSTYLE = -20;
		public const int GWL_HINSTANCE = -6;
		public const int GWL_ID = -12;
		public const int GWL_STYLE = -16;
		public const int GWL_USERDATA = -21;
		public const int GWL_WNDPROC = -4;
	

		// Window Styles
		public const UInt32 WS_OVERLAPPED = 0;
		public const UInt32 WS_POPUP = 0x80000000;
		public const UInt32 WS_CHILD = 0x40000000;
		public const UInt32 WS_MINIMIZE = 0x20000000;
		public const UInt32 WS_VISIBLE = 0x10000000;
		public const UInt32 WS_DISABLED = 0x8000000;
		public const UInt32 WS_CLIPSIBLINGS = 0x4000000;
		public const UInt32 WS_CLIPCHILDREN = 0x2000000;
		public const UInt32 WS_MAXIMIZE = 0x1000000;
		public const UInt32 WS_CAPTION = 0xC00000;
		// WS_BORDER or WS_DLGFRAME
		public const UInt32 WS_BORDER = 0x800000;
		public const UInt32 WS_DLGFRAME = 0x400000;
		public const UInt32 WS_VSCROLL = 0x200000;
		public const UInt32 WS_HSCROLL = 0x100000;
		public const UInt32 WS_SYSMENU = 0x80000;
		public const UInt32 WS_THICKFRAME = 0x40000;
		public const UInt32 WS_GROUP = 0x20000;
		public const UInt32 WS_TABSTOP = 0x10000;
		public const UInt32 WS_MINIMIZEBOX = 0x20000;
		public const UInt32 WS_MAXIMIZEBOX = 0x10000;
		public const UInt32 WS_TILED = WS_OVERLAPPED;
		public const UInt32 WS_ICONIC = WS_MINIMIZE;
		public const UInt32 WS_SIZEBOX = WS_THICKFRAME;

		// Extended Window Styles
		public const UInt32 WS_EX_DLGMODALFRAME = 0x0001;
		public const UInt32 WS_EX_NOPARENTNOTIFY = 0x0004;
		public const UInt32 WS_EX_TOPMOST = 0x0008;
		public const UInt32 WS_EX_ACCEPTFILES = 0x0010;
		public const UInt32 WS_EX_TRANSPARENT = 0x0020;
		public const UInt32 WS_EX_MDICHILD = 0x0040;
		public const UInt32 WS_EX_TOOLWINDOW = 0x0080;
		public const UInt32 WS_EX_WINDOWEDGE = 0x0100;
		public const UInt32 WS_EX_CLIENTEDGE = 0x0200;
		public const UInt32 WS_EX_CONTEXTHELP = 0x0400;
		public const UInt32 WS_EX_RIGHT = 0x1000;
		public const UInt32 WS_EX_LEFT = 0x0000;
		public const UInt32 WS_EX_RTLREADING = 0x2000;
		public const UInt32 WS_EX_LTRREADING = 0x0000;
		public const UInt32 WS_EX_LEFTSCROLLBAR = 0x4000;
		public const UInt32 WS_EX_RIGHTSCROLLBAR = 0x0000;
		public const UInt32 WS_EX_CONTROLPARENT = 0x10000;
		public const UInt32 WS_EX_STATICEDGE = 0x20000;
		public const UInt32 WS_EX_APPWINDOW = 0x40000;
		public const UInt32 WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
		public const UInt32 WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
		public const UInt32 WS_EX_LAYERED = 0x00080000;
		public const UInt32 WS_EX_NOINHERITLAYOUT = 0x00100000;
		// Disable inheritence of mirroring by children
		public const UInt32 WS_EX_LAYOUTRTL = 0x00400000;
		// Right to left mirroring
		public const UInt32 WS_EX_COMPOSITED = 0x02000000;
		public const UInt32 WS_EX_NOACTIVATE = 0x08000000;

		[DllImport("user32")]
		public static extern bool BringWindowToTop(HWND hwnd);

		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

		[DllImport("user32.dll", EntryPoint = "GetClassLong")]
		static extern uint GetClassLong32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
		static extern IntPtr GetClassLong64(IntPtr hWnd, int nIndex);

		/// <summary>
		/// 64 bit version maybe loses significant 64-bit specific information
		/// </summary>
		static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size == 4)
				return new IntPtr((long)GetClassLong32(hWnd, nIndex));
			else
				return GetClassLong64(hWnd, nIndex);
		}

		public const int SW_SHOWNORMAL = 1;
		public const int SW_SHOWMINIMIZED = 2;
		public const int SW_SHOWMAXIMIZED = 3;
		public const int SW_MINIMIZE = 6;
		public const int SW_MAXIMIZE = 3;
		public const int SW_SHOW = 5;
		public const int SW_RESTORE = 9;

		[DllImport("user32.dll")]
		public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		public static uint WM_GETICON = 0x007f;
		public static IntPtr ICON_SMALL2 = new IntPtr(2);
		public static IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
		public static int GCL_HICON = -14;

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int ExtractIconEx(string stExeFileName, int nIconIndex, ref IntPtr phiconLarge, ref IntPtr phiconSmall, int nIcons);

		public static System.Drawing.Icon GetSmallWindowIcon(IntPtr hWnd)
		{
			try {
				IntPtr hIcon = default(IntPtr);

				hIcon = SendMessage(hWnd, WM_GETICON, ICON_SMALL2, IntPtr.Zero);

				if (hIcon == IntPtr.Zero)
					hIcon = GetClassLongPtr(hWnd, GCL_HICON);

				if (hIcon == IntPtr.Zero)
					hIcon = LoadIcon(IntPtr.Zero, (IntPtr)0x7F00/*IDI_APPLICATION*/);

				if (hIcon != IntPtr.Zero)
					return System.Drawing.Icon.FromHandle(hIcon);
				else
					return null;
			} catch (Exception) {
				return null;
			}
		}

	}



}
