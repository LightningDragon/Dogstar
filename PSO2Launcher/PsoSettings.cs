using System;
using System.Collections.Generic;
using System.IO;
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
			get { return Get<bool>("ShaderQuality"); }
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
			var match = Regex.Match(_data, $@"\s*{name}\s*=\s*{{*""*(.+)""*}}*,");
			return match.Groups[1].Value;
		}

		private static void SetValue<T>(string name, T value)
		{
			try
			{
				LoadCheck();
				_data = Regex.Replace(_data, $@"(?<start>\s*{name}\s*=\s*{{*""*).+(?<end>""*}}*,)", $"${{start}}{value}${{end}}");
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
