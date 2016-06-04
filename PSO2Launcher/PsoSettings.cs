using System;
using System.Collections.Generic;
using System.IO;
using NLua;
using static Dogstar.Helper;

namespace Dogstar
{
	public static class PsoSettings
	{
		private static readonly Lua LuaVm = new Lua();
		private static readonly Dictionary<string, dynamic> Cache = new Dictionary<string, dynamic>();

		private static bool _isLoaded;

		public static int Vsync
		{
			get { return Get<int>("Ini.FrameKeep"); }
			set { Cache["Ini.FrameKeep"] = value; }
		}

		public static bool FullScreen
		{
			get { return Get<bool>("Ini.Windows.FullScreen"); }
			set { Cache["Ini.Windows.FullScreen"] = value; }
		}

		public static bool VirtualFullScreen
		{
			get { return Get<bool>("Ini.Windows.VirtualFullScreen"); }
			set { Cache["Ini.Windows.VirtualFullScreen"] = value; }
		}

		public static bool MoviePlay
		{
			get { return Get<bool>("Ini.Config.Basic.MoviePlay"); }
			set { Cache["Ini.Config.Basic.MoviePlay"] = value; }
		}

		public static int ShaderQuality
		{
			get { return Get<int>("Ini.Config.Draw.ShaderLevel"); }
			set { Cache["Ini.Config.Draw.ShaderLevel"] = value; }
		}

		public static int TextureResolution
		{
			get { return Get<int>("Ini.Config.Draw.TextureResolution"); }
			set { Cache["Ini.Config.Draw.TextureResolution"] = value; }
		}

		public static int InterfaceSize
		{
			get { return Get<int>("Ini.Config.Screen.InterfaceSize"); }
			set { Cache["Ini.Config.Screen.InterfaceSize"] = value; }
		}

		public static int Music
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Bgm"); }
			set { Cache["Ini.Config.Sound.Volume.Bgm"] = value; }
		}

		public static int Voice
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Voice"); }
			set { Cache["Ini.Config.Sound.Volume.Voice"] = value; }
		}

		public static int Video
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Movie"); }
			set { Cache["Ini.Config.Sound.Volume.Movie"] = value; }
		}

		public static int Sound
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Se"); }
			set { Cache["Ini.Config.Sound.Volume.Se"] = value; }
		}

		public static int WindowHight
		{
			get { return Get<int>("Ini.Windows.Height"); }
			set { Cache["Ini.Windows.Height"] = value; }
		}

		public static int WindowWidth
		{
			get { return Get<int>("Ini.Windows.Width"); }
			set { Cache["Ini.Windows.Width"] = value; }
		}

		static PsoSettings()
		{
			LuaVm.DoString(Properties.Resources.Lua_table_print);
			LuaVm.DoString(Properties.Resources.Lua_to_string);
		}

		private static T Get<T>(string name)
		{
			try
			{
				LoadCheck();
				dynamic result;

				if (!Cache.TryGetValue(name, out result))
				{
					Cache[name] = result = LuaVm[name];
				}

				return Convert.ChangeType(result, typeof(T));
			}
			catch
			{
				return default(T);
			}
		}

		private static void SetValue<T>(string name, T value)
		{
			try
			{
				LoadCheck();
				LuaVm[name] = value;
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
				Reload();
			}
		}

		public static void Reload()
		{
			LuaVm.DoFile(Path.Combine(GameConfigFolder, "user.pso2"));
			_isLoaded = true;
		}

		public static void Save()
		{
			Reload();

			foreach (var kvp in Cache)
			{
				SetValue(kvp.Key, kvp.Value);
			}

			LuaVm.DoString("WrapsIni = {Ini = Ini}");
			string result = (string)LuaVm.DoString("return to_string(WrapsIni)")[0];

			File.WriteAllText(Path.Combine(GameConfigFolder, "user.pso2"), result);
		}

	}
}
