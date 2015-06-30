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

namespace Microsoft.Win32
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
			Gtk.Application.Quit();

			if (code < 0)
				return CallNextHookEx(m_hhook, code, wParam, lParam);

			// we're gonna ignore peek messages
//			if ( code == Win32.HC_ACTION )
//				RaiseMouseHookEvent( wParam, CrackHookMsg( wParam, (Win32.MOUSEHOOKSTRUCT)Marshal.PtrToStructure( lParam, typeof( Win32.MOUSEHOOKSTRUCT ) ) ) );

			if (code == Win32.HC_ACTION) {
				Gtk.Application.Quit();
			}

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
	}

}
