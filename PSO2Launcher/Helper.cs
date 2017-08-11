using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using Dogstar.Properties;
using Dogstar.Resources;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Dogstar
{
	public static class Helper
	{
		public const MessageDialogStyle AffirmNeg = MessageDialogStyle.AffirmativeAndNegative;

		private static readonly string[] HexTable = Properties.Resources.HexTable.Split();

		private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		public static readonly MetroDialogSettings YesNo = new MetroDialogSettings { AffirmativeButtonText = Text.Yes, NegativeButtonText = Text.No };

		public static readonly MetroDialogSettings MovedDeleted = new MetroDialogSettings { AffirmativeButtonText = Text.Moved, NegativeButtonText = Text.Deleted };

		public static readonly Uri BasePrecede = new Uri("http://download.pso2.jp/patch_prod/patches_precede/");

		public static readonly Uri ManagementUrl = new Uri("http://patch01.pso2gs.net/patch_prod/patches/management_beta.txt");

		public static readonly AssemblyName ApplicationInfo = Application.Current.MainWindow.GetType().Assembly.GetName();

		public static readonly string HostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

		public static readonly string GameConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SEGA", "PHANTASYSTARONLINE2");

		public static readonly string LauncherListPath = Path.Combine(GameConfigFolder, "launcherlist.txt");

		public static readonly string PatchListPath = Path.Combine(GameConfigFolder, "_patchlist.txt");

		public static readonly string PatchListAlwaysPath = Path.Combine(GameConfigFolder, "_patchlist_always.txt");

		public static readonly string VersionPath = Path.Combine(GameConfigFolder, "version.ver");

		public static readonly string PrecedeTxtPath = Path.Combine(GameConfigFolder, "precede.txt");

		public static string DataFolder => Path.Combine(Settings.Default.GameFolder, "data", "win32");

		public static string PrecedeFolder => Path.Combine(Settings.Default.GameFolder, "_precede");

		public static Dictionary<string, string> ManagementData { get; private set; }

		public static string MakeLocalToGame(string fileName) => Path.Combine(Settings.Default.GameFolder, fileName);

		public static bool ProxyCheck() => File.ReadLines(HostsPath)
			.Select(x => x.Trim())
			.Any(x => !x.StartsWith("#", StringComparison.Ordinal) && x.Contains(".pso2gs.net"));

		public static IEnumerable<string> StripProxyEntries(IEnumerable<string> entries) => entries
			.Where(x => (x.StartsWith("#", StringComparison.Ordinal) || !x.Contains(".pso2gs.net")) && !x.StartsWith("# Dogstar", StringComparison.Ordinal));

		public static async Task PullManagementData()
		{
			using (var client = new AquaHttpClient())
			{
				client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

				ManagementData = (await client.DownloadStringTaskAsync(ManagementUrl))
					.LineSplit()
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.ToDictionary(x => x.Split('=')[0], y => y.Split('=')[1]);
			}
		}

		private static string HashFile(string file)
		{
			using (var stream = File.OpenRead(file))
			using (var buffstream = new BufferedStream(stream, 0x8000))
			{
				return string.Join("", MD5.Create().ComputeHash(buffstream).Select(b => HexTable[b]));
			}
		}

		public static string SizeSuffix(long value)
		{
			if (value < 0) { return "-" + SizeSuffix(-value); }
			if (value == 0) { return "0.0 bytes"; }

			var mag = (int)Math.Log(value, 1024);
			var adjustedSize = (decimal)value / (1L << (mag * 10));

			return $"{adjustedSize:n1} {SizeSuffixes[mag]}";
		}

		public static void CreateDirectoryIfNoneExists(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public static void DeleteFileIfItExists(string path)
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		public static void MoveAndOverwriteFile(string source, string destination)
		{
			if (File.Exists(destination))
			{
				File.Delete(destination);
			}
			File.Move(source, destination);
		}

		public static bool LaunchGame()
		{
			var info = new ProcessStartInfo(Path.Combine(Settings.Default.GameFolder, "pso2.exe"), "+0x33aca2b9") { UseShellExecute = false };
			info.EnvironmentVariables["-pso2"] = "+0x01e3f1e9";
			return new Process() { StartInfo = info }.Start();
		}

		public static string GetTweakerGameFolder()
		{
			using (var regkey = Registry.CurrentUser.OpenSubKey(@"Software\AIDA"))
			{
				return Convert.ToString(regkey?.GetValue("PSO2Dir"));
			}
		}

		public static void SetTweakerRemoteVersion(string version)
		{
			using (var regkey = Registry.CurrentUser.OpenSubKey(@"Software\AIDA", true))
			{
				if (regkey?.GetValue("PSO2RemoteVersion") != null)
				{
					try
					{
						regkey.SetValue("PSO2RemoteVersion", version);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
			}
		}

		public static IEnumerable<PatchListEntry> ParsePatchList(string list)
		{
			return from l in list.LineSplit() where !string.IsNullOrWhiteSpace(l) select new PatchListEntry(l);
		}

		public static bool IsFileUpToDate(string file, long size, string hash)
		{
			try
			{
				var info = new FileInfo(file);
				return info.Exists && info.Length == size && HashFile(file) == hash;
			}
			catch
			{
				return false;
			}
		}

		public static void RestorePatchBackup(string patchname)
		{
			var installPath = DataFolder;
			var path = Path.Combine(installPath, "backup", patchname);

			if (!Directory.Exists(path))
			{
				return;
			}

			foreach (var file in Directory.EnumerateFiles(path))
			{
				MoveAndOverwriteFile(file, Path.Combine(installPath, Path.GetFileName(file ?? string.Empty)));
			}

			Directory.Delete(path);
		}

		public static async Task RestoreAllPatchBackups()
		{
			await Task.Run(() =>
			{
				var path = Path.Combine(DataFolder, "backup");

				if (Directory.Exists(path))
				{
					RestorePatchBackup("JPECodes");
					RestorePatchBackup("JPEnemies");

					foreach (var entry in Directory.EnumerateDirectories(path))
					{
						RestorePatchBackup(entry);
					}
				}
			});
		}

		public static async Task<bool> IsGameUpToDate()
		{
			try
			{
				if (ManagementData == null)
				{
					await PullManagementData();
				}

				if (ManagementData == null)
				{
					return true;
				}

				var versionUrl = new Uri(new Uri(ManagementData["PatchURL"]), "version.ver");
				string localVersion = await Task.Run(() => File.Exists(VersionPath) ? File.ReadAllText(VersionPath) : string.Empty);

				using (var client = new AquaHttpClient())
				{
					client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
					string remoteVersion = await client.DownloadStringTaskAsync(versionUrl);
					await Task.Run(() => File.WriteAllText(Path.Combine(GameConfigFolder, "_version.ver"), remoteVersion));
					return localVersion == remoteVersion;
				}
			}
			catch
			{
				return true;
			}
		}

		public static async Task<bool> IsNewPrecedeAvailable()
		{
			await PullManagementData();

			if (ManagementData.ContainsKey("PrecedeVersion") && ManagementData.ContainsKey("PrecedeCurrent"))
			{
				var version = ManagementData["PrecedeVersion"];
				var listnum = ManagementData["PrecedeCurrent"];
				var current = await Task.Run(() => File.Exists(PrecedeTxtPath) ? File.ReadAllText(PrecedeTxtPath) : string.Empty);
				return string.IsNullOrEmpty(current) || current != $"{version}\t{listnum}";
			}

			return false;
		}

		public static void SavePluginSettings()
		{
			Settings.Default.PluginSettings = JsonConvert.SerializeObject(PluginManager.PluginSettings);
			Settings.Default.Save();
		}
	}
}
