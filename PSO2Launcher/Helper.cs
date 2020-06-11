using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using Dogstar.Properties;
using Dogstar.Resources;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

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
	}
}