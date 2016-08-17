using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Dogstar
{
	public static class External
	{
		[DllImport("user32.dll")]
		public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref Devmode devMode);

		public static bool FlashWindow(Window window, bool bInvert) => FlashWindow(new WindowInteropHelper(window).Handle, bInvert);

		public static IEnumerable<Devmode> GetDisplayModes(string device = null)
		{
			var index = 0;
			var mode = new Devmode();

			while (EnumDisplaySettings(device, index++, ref mode))
			{
				yield return mode;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct Devmode
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string dmDeviceName;
			public short dmSpecVersion;
			public short dmDriverVersion;
			public short dmSize;
			public short dmDriverExtra;
			public int dmFields;
			public int dmPositionX;
			public int dmPositionY;
			public int dmDisplayOrientation;
			public int dmDisplayFixedOutput;
			public short dmColor;
			public short dmDuplex;
			public short dmYResolution;
			public short dmTTOption;
			public short dmCollate;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string dmFormName;
			public short dmLogPixels;
			public int dmBitsPerPel;
			public int dmPelsWidth;
			public int dmPelsHeight;
			public int dmDisplayFlags;
			public int dmDisplayFrequency;
			public int dmICMMethod;
			public int dmICMIntent;
			public int dmMediaType;
			public int dmDitherType;
			public int dmReserved1;
			public int dmReserved2;
			public int dmPanningWidth;
			public int dmPanningHeight;
		}
	}
}
