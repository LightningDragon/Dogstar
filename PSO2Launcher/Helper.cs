using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

		public static readonly MetroDialogSettings YesNo = new MetroDialogSettings { AffirmativeButtonText = Text.Yes, NegativeButtonText = Text.No };

		public static readonly MetroDialogSettings MovedDeleted = new MetroDialogSettings { AffirmativeButtonText = Text.Moved, NegativeButtonText = Text.Deleted };

		public static readonly AssemblyName ApplicationInfo = Application.Current.MainWindow?.GetType().Assembly.GetName();

		public static readonly string HostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

		public static string MakeLocalToGame(string fileName) => Path.Combine(Settings.Default.GameFolder, fileName);

		public static bool ProxyCheck() => File.ReadLines(HostsPath)
			.Select(x => x.Trim())
			.Any(x => !x.StartsWith("#", StringComparison.Ordinal) && x.Contains(".pso2gs.net"));

		public static IEnumerable<string> StripProxyEntries(IEnumerable<string> entries) => entries
			.Where(x => (x.StartsWith("#", StringComparison.Ordinal) || !x.Contains(".pso2gs.net")) && !x.StartsWith("# Dogstar", StringComparison.Ordinal));

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
			const int magic = 0x32315350; // 'PS12'

			int time = Environment.TickCount & int.MaxValue;
			int arg = time ^ magic;

			var info = new ProcessStartInfo(Path.Combine(Settings.Default.GameFolder, "pso2.exe"), $"+0x{arg:x8}")
			{
				UseShellExecute = false
			};

			info.EnvironmentVariables["-pso2"] = $"+0x{time:x8}";
			return new Process { StartInfo = info }.Start();
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

		public static bool IsFileUpToDate(string filePath, long targetSize, string targetHash)
		{
			if (filePath.Contains("pso2launcher"))
				return true;
			try
			{
				var info = new FileInfo(filePath);
				return info.Exists && info.Length == targetSize && HashFile(filePath) == targetHash;
			}
			catch
			{
				return false;
			}
		}

		public static void RestorePatchBackup(string patchname)
		{
			var installPath = PatchProvider.DataFolder; // UNDONE: THIS WILL NOT WORK WITH BOTH VERSIONS
			var path        = Path.Combine(installPath, "backup", patchname);

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
				var path = Path.Combine( /* UNDONE: THIS WILL NOT WORK WITH BOTH VERSIONS */
				                        PatchProvider.DataFolder,
				                        "backup");

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

		public static void SavePluginSettings()
		{
			Settings.Default.PluginSettings = JsonConvert.SerializeObject(PluginManager.PluginSettings);
			Settings.Default.Save();
		}
	}
}