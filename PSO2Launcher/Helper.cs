using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using SharpCompress.Reader;
using Newtonsoft.Json;
using Microsoft.Win32;
using MahApps.Metro.Controls.Dialogs;
using DogStar.Resources;
using DogStar.Properties;

namespace DogStar
{
	public static class Helper
	{
		public const MessageDialogStyle AffirmNeg = MessageDialogStyle.AffirmativeAndNegative;

		private static readonly string[] HexTable = Properties.Resources.HexTable.Split();

		private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

		private static string DataFolder => Path.Combine(Settings.Default.GameFolder, "data", "win32");

		public static readonly MetroDialogSettings YesNo = new MetroDialogSettings { AffirmativeButtonText = Text.Yes, NegativeButtonText = Text.No };

		public static readonly MetroDialogSettings MovedDeleted = new MetroDialogSettings { AffirmativeButtonText = Text.Moved, NegativeButtonText = Text.Deleted };

		public static readonly Uri BasePatch = new Uri("http://download.pso2.jp/patch_prod/patches/");

		public static readonly Uri BasePatchOld = new Uri("http://download.pso2.jp/patch_prod/patches_old/");

		public static readonly Uri PatchListOldUrl = new Uri("http://download.pso2.jp/patch_prod/patches_old/patchlist.txt");

		public static readonly Uri LauncherListUrl = new Uri("http://download.pso2.jp/patch_prod/patches/launcherlist.txt");

		public static readonly Uri PatchListUrl = new Uri("http://download.pso2.jp/patch_prod/patches/patchlist.txt");

		public static readonly Uri Arghlex = new Uri("http://pitchblack.arghlex.net/pso2/");

		public static readonly Uri VersionUrl = new Uri(BasePatch, "version.ver");

		public static readonly Uri ManagementUrl = new Uri("http://patch01.pso2gs.net/patch_prod/patches/management_beta.txt");

		public static readonly AssemblyName ApplicationInfo = Application.Current.MainWindow.GetType().Assembly.GetName();

		public static readonly string HostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

		public static readonly string GameConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SEGA", "PHANTASYSTARONLINE2");

		public static readonly string LauncherListPath = Path.Combine(GameConfigFolder, "launcherlist.txt");

		public static readonly string PatchListPath = Path.Combine(GameConfigFolder, "_patchlist.txt");

		public static readonly string PatchListOldPath = Path.Combine(GameConfigFolder, "_patchlist_old.txt");

		public static readonly string VersionPath = Path.Combine(GameConfigFolder, "version.ver");

		public static AquaHttpClient AquaClient => new AquaHttpClient();

		public static string MakeLocalToGame(string fileName) => Path.Combine(Settings.Default.GameFolder, fileName);

		public static bool ProxyCheck() => File.ReadLines(HostsPath).Any(x => !x.StartsWith("#") && x.Contains(".pso2gs.net"));

		public static IEnumerable<string> StripProxyEntries(IEnumerable<string> entries) => entries.Where(x => (x.StartsWith("#") || !x.Contains(".pso2gs.net")) && !x.StartsWith("# Dogstar"));

		static string SizeSuffix(long value)
		{
			if (value < 0) { return "-" + SizeSuffix(-value); }
			if (value == 0) { return "0.0 bytes"; }

			int mag = (int)Math.Log(value, 1024);
			decimal adjustedSize = (decimal)value / (1L << (mag * 10));

			return $"{adjustedSize:n1} {SizeSuffixes[mag]}";
		}

		private static string HashFile(string file)
		{
			using (var stream = File.OpenRead(file))
			using (var buffstream = new BufferedStream(stream, 0x8000))
			{
				return string.Join("", MD5.Create().ComputeHash(buffstream).Select(b => HexTable[b]));
			}
		}

		public static void CreateDirectoryIfNoneExists(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		private static void MoveAndOverwriteFile(string source, string destination)
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

		public static IEnumerable<PatchListEntry> ParsePatchList(string list)
		{
			return from l in list.LineSplit() where !string.IsNullOrWhiteSpace(l) let data = l.Split('\t') select new PatchListEntry(data[0], data[1], data[2]);
		}

		public static async Task<dynamic> GetArghlexJson()
		{
			using (var client = AquaClient)
			{
				var json = await client.DownloadStringTaskAsync(new Uri(Arghlex, "?sort=modtime&order=desc&json"));
				return JsonConvert.DeserializeObject(json);
			}
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

		public static async Task DownloadPatchFile(string relativeAddress, string filePath, ProgressBar bar, Label label)
		{
			await Task.Run(() => CreateDirectoryIfNoneExists(Path.GetDirectoryName(filePath)));

			using (var client = AquaClient)
			{
				client.DownloadProgressChanged += (s, e) =>
				{
					bar.Dispatcher.InvokeAsync(() =>
					{
						bar.Maximum = 100;
						bar.Value = e.ProgressPercentage;
						label.Content = $"{SizeSuffix(e.BytesReceived)}/{SizeSuffix(e.TotalBytesToReceive)}";
					});
				};

				client.DownloadFileCompleted += (s, e) =>
				{
					bar.Dispatcher.InvokeAsync(() => bar.Value = 100);
					MoveAndOverwriteFile(filePath, Path.ChangeExtension(filePath, null));
				};

				try
				{
					await client.DownloadFileTaskAsync(new Uri(BasePatch, relativeAddress), filePath);
				}
				catch
				{
					await client.DownloadFileTaskAsync(new Uri(BasePatchOld, relativeAddress), filePath);
				}
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
				string path = Path.Combine(DataFolder, "backup");
				if (Directory.Exists(path))
				{
					foreach (var entry in Directory.EnumerateDirectories(path))
					{
						RestorePatchBackup(entry);
					}
				}
			});

			Settings.Default.InstalledEnglishPatch = 0;
			Settings.Default.InstalledLargeFiles = 0;
			Settings.Default.InstalledJPECodes = 0;
			Settings.Default.InstalledJPEnemies = 0;
			// TODO: story patch
		}

		public static bool InstallPatch(string filename, string patchname)
		{
			if (!File.Exists(filename))
			{
				return false;
			}

			var installPath = DataFolder;
			var backupPath = Path.Combine(installPath, "backup", patchname);

			if (!Directory.Exists(backupPath))
			{
				Directory.CreateDirectory(backupPath);
			}

			try
			{
				using (Stream stream = File.OpenRead(filename))
				{
					using (var reader = ReaderFactory.Open(stream))
					{
						while (reader.MoveToNextEntry() && !reader.Entry.IsDirectory)
						{
							var file = Path.Combine(installPath, reader.Entry.Key);
							var backup = Path.Combine(backupPath, reader.Entry.Key);

							if (File.Exists(file) && !File.Exists(backup))
							{
								File.Move(file, backup);
							}

							reader.WriteEntryToDirectory(installPath);
						}
					}
				}
			}
			catch (Exception)
			{
				RestorePatchBackup(patchname);
				return false;
			}

			return true;
		}

		public static async Task<bool> IsGameUpToDate()
		{
		    try
		    {
		        var version = await Task.Run(() => File.Exists(VersionPath) ? File.ReadAllText(VersionPath) : string.Empty);
		        using (var client = AquaClient)
		        {
		            return version == await client.DownloadStringTaskAsync(VersionUrl);
		        }
		    }
		    catch
		    {
		        return false;
		    }
		}
	}
}
