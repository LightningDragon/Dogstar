using System;
using System.IO;
using System.Text.RegularExpressions;
using static Dogstar.Helper;

namespace Dogstar
{
	public static class PsoSettings
	{
		private static bool _isLoaded;
		private static string _data;

		public static dynamic Vsync
		{
			get { return Get<float>("FrameKeep"); }
			set { Set("FrameKeep", value); }
		}

		public static dynamic FullScreen
		{
			get { return Get<bool>("FullScreen"); }
			set { Set("FullScreen", value); }
		}

		public static dynamic VirtualFullScreen
		{
			get { return Get<bool>("VirtualFullScreen"); }
			set { Set("VirtualFullScreen", value); }
		}

		public static dynamic MoviePlay
		{
			get { return Get<bool>("MoviePlay"); }
			set { Set("MoviePlay", value); }
		}

		private static T Get<T>(string name)
		{
			try
			{
				LoadCheck();
				var match = Regex.Match(_data, $"\\s*{name}\\s*=\\s*{{*\"*(.+)\"*}}*,");
				return (T)Convert.ChangeType(match.Groups[1].Value, typeof(T));
			}
			catch
			{
				return default(T);
			}
		}

		private static void Set<T>(string name, T value)
		{
			try
			{
				LoadCheck();
				_data = Regex.Replace(_data, $"(?<start>\\s*{name}\\s*=\\s*{{*\"*).+(?<end>\"*}}*,)", $"${{start}}{value}${{end}}");
			}
			catch
			{
			}
		}

		private static void LoadCheck()
		{
			if (!_isLoaded)
			{
				_data = File.ReadAllText(Path.Combine(GameConfigFolder, "user.pso2"));
				_isLoaded = true;
			}
		}

		public static void Save()
		{
			File.WriteAllText(Path.Combine(GameConfigFolder, "user.pso2"), _data);
		}
	}
}
