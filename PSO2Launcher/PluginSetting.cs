using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSO2H;
using System.IO;
using Dogstar.Properties;

namespace Dogstar
{
	public class PluginSetting
	{

		// TODO: Change to custom download function
		// TODO: Remove test things

		private static List<PluginSetting> PluginSettings = new List<PluginSetting>();

		public readonly Plugin Plugin;
		public bool isChecked;

		#region For Testing
		static PluginSetting()
		{
			string TestJSON = @"{""Name"":""PSO2DamageDump"", ""CurrentVersion"": 1.0, ""Description"": ""A damage dumper for PSO2, for use with ACT and other analysis tools"", ""Plugin"": ""http://vxyz.me/files/Plugins/PSO2DamageDump/PSO2DamageDump.dll"", ""Configuration"": ""http://vxyz.me/files/Plugins/PSO2DamageDump/PSO2DamageDump.cfg""}";
			new PluginSetting(TestJSON, 1.0, true);
		}
		#endregion

		//Returns all plugin settings
		public static IEnumerable<PluginSetting> GetAllPluginSettings()
		{
			return PluginSettings.AsEnumerable();
		}

		public PluginSetting(string json, double currVersion, bool enabled = true)
		{
			string errMsg = "";
			isChecked = enabled;
			Plugin = new Plugin(json, Path.Combine(Settings.Default.GameFolder, "Plugins"), out errMsg, currVersion, (x, y) => Plugin.DownloadStatus.Success);
			PluginSettings.Add(this);
		}
	}
}
