using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Dogstar.Helper;

namespace Dogstar
{
	public static class PsoSettings
	{
		private static readonly Dictionary<string, dynamic> Cashe = new Dictionary<string, dynamic>();

		private static bool _isLoaded;
		private static string _data;

		public static dynamic Vsync
		{
			get { return Get<float>("FrameKeep"); }
			set { Cashe["FrameKeep"] = value; }
		}

		public static dynamic FullScreen
		{
			get { return Get<bool>("FullScreen"); }
			set { Cashe["FullScreen"] = value; }
		}

		public static dynamic VirtualFullScreen
		{
			get { return Get<bool>("VirtualFullScreen"); }
			set { Cashe["VirtualFullScreen"] = value; }
		}

		public static dynamic MoviePlay
		{
			get { return Get<bool>("MoviePlay"); }
			set { Cashe["MoviePlay"] = value; }
		}

		public static dynamic ShaderQuality
		{
			get { return Get<int>("ShaderQuality"); }
			set { Cashe["ShaderQuality"] = value; }
		}

		public static dynamic TextureResolution
		{
			get { return Get<int>("TextureResolution"); }
			set { Cashe["TextureResolution"] = value; }
		}

		public static dynamic InterfaceSize
		{
			get { return Get<int>("InterfaceSize"); }
			set { Cashe["InterfaceSize"] = value; }
		}

		public static dynamic Music
		{
			get { return Get<int>("Bgm"); }
			set { Cashe["Bgm"] = value; }
		}

		public static dynamic Voice
		{
			get { return Get<int>("Voice"); }
			set { Cashe["Voice"] = value; }
		}

		public static dynamic Video
		{
			get { return Get<int>("Movie"); }
			set { Cashe["Movie"] = value; }
		}

		public static dynamic Sound
		{
			get { return Get<int>("Se"); }
			set { Cashe["Se"] = value; }
		}

		public static dynamic WindowHight
		{
			get { return Get<int>("Windows.Height"); }
			set { Cashe["Windows.Height"] = value; }
		}

		public static dynamic WindowWidth
		{
			get { return Get<int>("Windows.Width"); }
			set { Cashe["Windows.Width"] = value; }
		}

		private static T Get<T>(string name)
		{
			try
			{
				dynamic result;

				if (!Cashe.TryGetValue(name, out result))
				{
					Cashe[name] = result = GetValue(name);
				}

				return Convert.ChangeType(result, typeof(T));
			}
			catch
			{
				return default(T);
			}
		}

		private static string GetValue(string name)
		{
			LoadCheck();
			var data = _data;
			var subStrings = name.Split('.');

			for (int index = 1; index < subStrings.Length; index++)
			{
				data = Regex.Match(data, $"{subStrings[index - 1]}.+", RegexOptions.Singleline).Value;
			}

			var match = Regex.Match(data, $@"\s*{subStrings.Last()}\s*=\s*{{*""*(.+)""*}}*,");
			return match.Groups[1].Value;
		}

		private static void SetValue<T>(string name, T value)
		{
			try
			{
				LoadCheck();
				var result = _data;
				var subStrings = name.Split('.');
				var replacementTrace = new string[subStrings.Length];

				for (int index = 1; index < subStrings.Length; index++)
				{
					replacementTrace[index - 1] = result;
					result = Regex.Match(result, $"{subStrings[index - 1]}.+", RegexOptions.Singleline).Value;
				}

				replacementTrace[replacementTrace.Length - 1] = result;
				result = Regex.Replace(result, $@"(?<start>\s*{subStrings.Last()}\s*=\s*{{*""*).+(?<end>""*}}*,)", $"${{start}}{value}${{end}}");

				for (int index = replacementTrace.Length - 1; index > 0; index--)
				{
					result = replacementTrace[index - 1].Replace(replacementTrace[index], result);
				}

				_data = result;
			}
			catch
			{
				// ignored
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

		public static async Task Reload()
		{
			_data = await Task.Run(() => File.ReadAllText(Path.Combine(GameConfigFolder, "user.pso2")));
			_isLoaded = true;
		}

		public static async Task Save()
		{
			await Reload();

			foreach (var kvp in Cashe)
			{
				SetValue(kvp.Key, kvp.Value);
			}

			await Task.Run(() => File.WriteAllText(Path.Combine(GameConfigFolder, "user.pso2"), _data));
		}
	}
}
