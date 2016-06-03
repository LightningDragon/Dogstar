﻿using System;
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
		private static readonly Dictionary<string, dynamic> Cache = new Dictionary<string, dynamic>();

		private static bool _isLoaded;

		public static dynamic Vsync
		{
			get { return Get<float>("Ini.FrameKeep"); }
			set { Cache["Ini.FrameKeep"] = value; }
		}

		public static dynamic FullScreen
		{
			get { return Get<bool>("Ini.Windows.FullScreen"); }
			set { Cache["Ini.Windows.FullScreen"] = value; }
		}

		public static dynamic VirtualFullScreen
		{
			get { return Get<bool>("Ini.Windows.VirtualFullScreen"); }
			set { Cache["Ini.Windows.VirtualFullScreen"] = value; }
		}

		public static dynamic MoviePlay
		{
			get { return Get<bool>("Ini.Config.Basic.MoviePlay"); }
			set { Cache["Ini.Config.Basic.MoviePlay"] = value; }
		}

		public static dynamic ShaderQuality
		{
			get { return Get<int>("Ini.Config.Draw.ShaderLevel"); }
			set { Cache["Ini.Config.Draw.ShaderLevel"] = value; }
		}

		public static dynamic TextureResolution
		{
			get { return Get<int>("Ini.Config.Draw.TextureResolution"); }
			set { Cache["Ini.Config.Draw.TextureResolution"] = value; }
		}

		public static dynamic InterfaceSize
		{
			get { return Get<int>("Ini.Config.Screen.InterfaceSize"); }
			set { Cache["Ini.Config.Screen.InterfaceSize"] = value; }
		}

		public static dynamic Music
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Bgm"); }
			set { Cache["Ini.Config.Sound.Volume.Bgm"] = value; }
		}

		public static dynamic Voice
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Voice"); }
			set { Cache["Ini.Config.Sound.Volume.Voice"] = value; }
		}

		public static dynamic Video
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Movie"); }
			set { Cache["Ini.Config.Sound.Volume.Movie"] = value; }
		}

		public static dynamic Sound
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Se"); }
			set { Cache["Ini.Config.Sound.Volume.Se"] = value; }
		}

		public static dynamic WindowHight
		{
			get { return Get<int>("Ini.Windows.Height"); }
			set { Cache["Ini.Windows.Height"] = value; }
		}

		public static dynamic WindowWidth
		{
			get { return Get<int>("Ini.Windows.Width"); }
			set { Cache["Ini.Windows.Width"] = value; }
		}

		private static T Get<T>(string name)
		{
			try
			{
				LoadCheck();
				dynamic result;

				if (!Cache.TryGetValue(name, out result))
				{
					Cache[name] = result = luaVM.GetString(name);
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

			foreach (var kvp in Cache)
			{
				SetValue(kvp.Key, kvp.Value);
			}

			luaVM.DoString("LuaSave(ummmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm)");
		}

		[LuaAccessible]
		public static async Task LuaSave(string data)
		{
			await Task.Run(() => File.WriteAllText(Path.Combine(GameConfigFolder, "user.pso2"), data));
		}
	}
}
