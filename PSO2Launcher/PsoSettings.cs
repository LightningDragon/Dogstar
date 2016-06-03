using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LuaInterface;
using static Dogstar.Helper;

namespace Dogstar
{
	public static class PsoSettings
	{
		private static readonly Lua luaVM = new Lua();
		private static readonly Dictionary<string, dynamic> Cashe = new Dictionary<string, dynamic>();

		private static bool _isLoaded;

		public static dynamic Vsync
		{
			get { return Get<float>("Ini.FrameKeep"); }
			set { Cashe["Ini.FrameKeep"] = value; }
		}

		public static dynamic FullScreen
		{
			get { return Get<bool>("Ini.Windows.FullScreen"); }
			set { Cashe["Ini.Windows.FullScreen"] = value; }
		}

		public static dynamic VirtualFullScreen
		{
			get { return Get<bool>("Ini.Windows.VirtualFullScreen"); }
			set { Cashe["Ini.Windows.VirtualFullScreen"] = value; }
		}

		public static dynamic MoviePlay
		{
			get { return Get<bool>("Ini.Config.Basic.MoviePlay"); }
			set { Cashe["Ini.Config.Basic.MoviePlay"] = value; }
		}

		public static dynamic ShaderQuality
		{
			get { return Get<int>("Ini.Config.Draw.ShaderLevel"); }
			set { Cashe["Ini.Config.Draw.ShaderLevel"] = value; }
		}

		public static dynamic TextureResolution
		{
			get { return Get<int>("Ini.Config.Draw.TextureResolution"); }
			set { Cashe["Ini.Config.Draw.TextureResolution"] = value; }
		}

		public static dynamic InterfaceSize
		{
			get { return Get<int>("Ini.Config.Screen.InterfaceSize"); }
			set { Cashe["Ini.Config.Screen.InterfaceSize"] = value; }
		}

		public static dynamic Music
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Bgm"); }
			set { Cashe["Ini.Config.Sound.Volume.Bgm"] = value; }
		}

		public static dynamic Voice
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Voice"); }
			set { Cashe["Ini.Config.Sound.Volume.Voice"] = value; }
		}

		public static dynamic Video
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Movie"); }
			set { Cashe["Ini.Config.Sound.Volume.Movie"] = value; }
		}

		public static dynamic Sound
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Se"); }
			set { Cashe["Ini.Config.Sound.Volume.Se"] = value; }
		}

		public static dynamic WindowHight
		{
			get { return Get<int>("Ini.Windows.Height"); }
			set { Cashe["Ini.Windows.Height"] = value; }
		}

		public static dynamic WindowWidth
		{
			get { return Get<int>("Ini.Windows.Width"); }
			set { Cashe["Ini.Windows.Width"] = value; }
		}

        static PsoSettings()
        {
            luaVM.DoString(Properties.Resources.Lua_table_print);
            luaVM.DoString(Properties.Resources.Lua_to_string);
        }

		private static T Get<T>(string name)
		{
			try
			{
				LoadCheck();
				dynamic result;

				if (!Cashe.TryGetValue(name, out result))
				{
					luaVM.DoString($@"GetValue(""{name}"", {name})");
					result = Cashe[name];
				}

				return Convert.ChangeType(result, typeof(T));
			}
			catch
			{
				return default(T);
			}
		}

		[LuaAccessible]
		private static void GetValue(string name, string data)
		{
			Cashe[name] = data;
		}

		private static void SetValue<T>(string name, T value)
		{
			try
			{
				LoadCheck();
				luaVM.DoString($"{name} = {value}");
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
				luaVM.DoFile(Path.Combine(GameConfigFolder, "user.pso2"));
				_isLoaded = true;
			}
		}

		public static async Task Reload()
		{
			await Task.Run(() => luaVM.DoFile(Path.Combine(GameConfigFolder, "user.pso2")));
			_isLoaded = true;
		}

		public static async Task Save()
		{
			await Reload();

			foreach (var kvp in Cashe)
			{
				SetValue(kvp.Key, kvp.Value);
			}
            luaVM.DoString("WrapsIni = Ini");
			luaVM.DoString("WrapsIni = to_string(WrapsIni))");
            await Task.Run(() => File.WriteAllText(Path.Combine(GameConfigFolder, "user.pso2"), luaVM["WrapsIni"].ToString()));
        }

	}
}
