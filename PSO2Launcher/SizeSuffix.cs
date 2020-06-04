using System;

namespace Dogstar
{
	public static class SizeSuffix
	{
		static readonly string[] sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		public static string GetSizeSuffix(long value, int decimalPlaces = 1)
		{
			if (value < 0)
			{
				return "-" + GetSizeSuffix(-value);
			}

			string formatString = $"{{0:n{decimalPlaces}}} {{1}}";

			if (value == 0)
			{
				return string.Format(formatString, 0, sizeSuffixes[0]);
			}

			var mag          = (int)Math.Log(value, 1024);
			var adjustedSize = (decimal)value / (1L << (mag * 10));

			return string.Format(formatString, adjustedSize, sizeSuffixes[mag]);
		}
	}
}