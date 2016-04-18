using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Dogstar
{
	public static class External
	{
		[DllImport("user32.dll")]
		public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

		public static bool FlashWindow(Window window, bool bInvert) => FlashWindow(new WindowInteropHelper(window).Handle, bInvert);
	}
}
