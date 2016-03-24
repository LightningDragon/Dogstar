using System;

namespace DogStar
{
	public static class Extensions
	{
		public static string[] LineSplit(this string str) => str.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
	}
}
