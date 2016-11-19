using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using static Dogstar.Helper;

namespace Dogstar
{
	public static class PluginManager
	{
		public static readonly List<PluginInfo> PluginSettings = new List<PluginInfo>();

		public static DownloadManager DownloadManager;

		public static async Task<PluginInfo> InfoFromUrl(Uri url)
		{
			var json = await DownloadManager.DownloadStringTaskAsync(url);
			PluginInfo result = await Task.Run(() => JsonConvert.DeserializeObject<PluginInfo>(json));
			result.Url = url;
			return result;
		}

		public static Task Install(PluginInfo info)
		{
			var pluginsFolder = MakeLocalToGame("Plugins");
			CreateDirectoryIfNoneExists(pluginsFolder);
			var result = DownloadManager.DownloadFileTaskAsync(info.Plugin, Path.Combine(pluginsFolder, Path.ChangeExtension(info.Name, ".dll")));
			info.IsEnabled = true;
			return result;
			//TODO: Config stuff
		}

		public static void Uninstall(PluginInfo info)
		{
			var pluginsFolder = MakeLocalToGame("Plugins");
			var disabledFolder = Path.Combine(pluginsFolder, "disabled");
			var enabledfilePath = Path.Combine(pluginsFolder, Path.ChangeExtension(info.Name, ".dll"));
			var disabledFilePath = Path.Combine(disabledFolder, Path.GetFileName(enabledfilePath));

			DeleteFileIfItExists(enabledfilePath);
			DeleteFileIfItExists(disabledFilePath);
			//TODO: Config stuff
		}

		public static void Disable(PluginInfo info)
		{
			var pluginsFolder = MakeLocalToGame("Plugins");
			var disabledFolder = Path.Combine(pluginsFolder, "disabled");
			var enabledfilePath = Path.Combine(pluginsFolder, Path.ChangeExtension(info.Name, ".dll"));
			var disabledFilePath = Path.Combine(disabledFolder, Path.GetFileName(enabledfilePath));

			CreateDirectoryIfNoneExists(pluginsFolder);
			CreateDirectoryIfNoneExists(pluginsFolder);

			if (File.Exists(enabledfilePath))
			{
				MoveAndOverwriteFile(enabledfilePath, disabledFilePath);
			}

			info.IsEnabled = false;
		}

		public static void Enable(PluginInfo info)
		{
			var pluginsFolder = MakeLocalToGame("Plugins");
			var disabledFolder = Path.Combine(pluginsFolder, "disabled");
			var enabledfilePath = Path.Combine(pluginsFolder, Path.ChangeExtension(info.Name, ".dll"));
			var disabledFilePath = Path.Combine(disabledFolder, Path.GetFileName(enabledfilePath));

			CreateDirectoryIfNoneExists(pluginsFolder);
			CreateDirectoryIfNoneExists(pluginsFolder);

			if (File.Exists(disabledFilePath))
			{
				MoveAndOverwriteFile(disabledFilePath, enabledfilePath);
			}

			info.IsEnabled = true;
		}
	}
}
