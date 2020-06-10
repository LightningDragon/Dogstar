using System;
using System.Collections.Generic;
using System.IO;
using Dogstar.GameEditionManagement;
using NLua;

namespace Dogstar
{
	public class PsoSettings
	{
		readonly Lua LuaVm = new Lua();
		readonly Dictionary<string, dynamic> Cache = new Dictionary<string, dynamic>();
		GameEditionManager edition;

		bool _isLoaded;

		public int Vsync
		{
			get { return Get<int>("Ini.FrameKeep"); }
			set { Cache["Ini.FrameKeep"] = value; }
		}

		public bool FullScreen
		{
			get { return Get<bool>("Ini.Windows.FullScreen"); }
			set { Cache["Ini.Windows.FullScreen"] = value; }
		}

		public bool VirtualFullScreen
		{
			get { return Get<bool>("Ini.Windows.VirtualFullScreen"); }
			set { Cache["Ini.Windows.VirtualFullScreen"] = value; }
		}

		public bool MoviePlay
		{
			get { return Get<bool>("Ini.Config.Basic.MoviePlay"); }
			set { Cache["Ini.Config.Basic.MoviePlay"] = value; }
		}

		public int ShaderQuality
		{
			get { return Get<int>("Ini.Config.Draw.ShaderLevel"); }
			set { Cache["Ini.Config.Draw.ShaderLevel"] = value; }
		}

		public int TextureResolution
		{
			get { return Get<int>("Ini.Config.Draw.TextureResolution"); }
			set { Cache["Ini.Config.Draw.TextureResolution"] = value; }
		}

		public int InterfaceSize
		{
			get { return Get<int>("Ini.Config.Screen.InterfaceSize"); }
			set { Cache["Ini.Config.Screen.InterfaceSize"] = value; }
		}

		public bool Surround
		{
			get { return Get<bool>("Ini.Config.Sound.Play.Surround"); }
			set { Cache["Ini.Config.Sound.Play.Surround"] = value; }
		}

		public bool GlobalFocus
		{
			get { return Get<bool>("Ini.Config.Sound.Play.GlobalFocus"); }
			set { Cache["Ini.Config.Sound.Play.GlobalFocus"] = value; }
		}

		public int Music
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Bgm"); }
			set { Cache["Ini.Config.Sound.Volume.Bgm"] = value; }
		}

		public int Voice
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Voice"); }
			set { Cache["Ini.Config.Sound.Volume.Voice"] = value; }
		}

		public int Video
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Movie"); }
			set { Cache["Ini.Config.Sound.Volume.Movie"] = value; }
		}

		public int Sound
		{
			get { return Get<int>("Ini.Config.Sound.Volume.Se"); }
			set { Cache["Ini.Config.Sound.Volume.Se"] = value; }
		}

		public int WindowHight
		{
			get { return Get<int>("Ini.Windows.Height"); }
			set { Cache["Ini.Windows.Height"] = value; }
		}

		public int WindowWidth
		{
			get { return Get<int>("Ini.Windows.Width"); }
			set { Cache["Ini.Windows.Width"] = value; }
		}

		public PsoSettings(GameEditionManager edition)
		{
			this.edition = edition;
			LuaVm.DoString(Properties.Resources.Lua_table_print);
			LuaVm.DoString(Properties.Resources.Lua_to_string);
		}

		T Get<T>(string name)
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

		void SetValue<T>(string name, T value)
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

		void LoadCheck()
		{
			if (!_isLoaded)
			{
				Reload();
			}
		}

		public void Reload()
		{
			LuaVm.DoFile(edition.PathProvider.ConfigurationFilePath);
			_isLoaded = true;
		}

		public void Save()
		{
			Reload();

			foreach (var kvp in Cache)
			{
				SetValue(kvp.Key, kvp.Value);
			}

			LuaVm.DoString("WrapsIni = {Ini = Ini}");
			var result = (string)LuaVm.DoString("return to_string(WrapsIni)")[0];

			File.WriteAllText(edition.PathProvider.ConfigurationFilePath, result);
		}

	}
}
