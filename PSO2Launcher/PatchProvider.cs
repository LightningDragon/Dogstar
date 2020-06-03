using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;
using Dogstar.Properties;

namespace Dogstar
{
	// TODO: rename - it does more than provide patch information
	public abstract class PatchProvider
	{
		public Dictionary<string, string> ManagementData { get; private set; }

		public abstract Uri BasePrecede   { get; }
		public abstract Uri ManagementUrl { get; }

		public abstract string GameConfigFolder { get; }

		public string LauncherListPath    => Path.Combine(GameConfigFolder, "launcherlist.txt");
		public string PatchListPath       => Path.Combine(GameConfigFolder, "_patchlist.txt");
		public string PatchListAlwaysPath => Path.Combine(GameConfigFolder, "_patchlist_always_win10.txt");
		public string VersionFilePath     => Path.Combine(GameConfigFolder, "version.ver");
		public string PrecedeTxtPath      => Path.Combine(GameConfigFolder, "precede.txt");

		// UNDONE: make Settings.Default.GameFolder selectable for region
		public static string DataFolder => Path.Combine(Settings.Default.GameFolder, "data", "win32");

		// UNDONE: make Settings.Default.GameFolder selectable for region
		public static string PrecedeFolder => Path.Combine(Settings.Default.GameFolder, "_precede");

		public Uri MasterUrl { get; private set; }
		public Uri PatchUrl  { get; private set; }

		public Uri LauncherListUrl    { get; private set; }
		public Uri PatchListUrl       { get; private set; }
		public Uri PatchListAlwaysUrl { get; private set; }
		// This probably should be some sort of "editions" array
		public Uri PatchListWin10Url  { get; private set; }
		public Uri VersionFileUrl     { get; private set; }

		/// <summary>
		/// Pull latest management data, including maintenance status, etc.
		/// </summary>
		public async Task PullManagementData()
		{
			using (var client = new AquaHttpClient())
			{
				client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

				ManagementData = (await client.DownloadStringTaskAsync(ManagementUrl))
					.LineSplit()
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Select(x => x.Split('='))
					.ToDictionary(key => key[0].Trim(), value => value[1].Trim());
			}

			MasterUrl = new Uri(ManagementData["MasterURL"]);
			PatchUrl  = new Uri(ManagementData["PatchURL"]);

			LauncherListUrl    = new Uri(PatchUrl, "launcherlist.txt");
			PatchListUrl       = new Uri(PatchUrl, "patchlist.txt");
			PatchListAlwaysUrl = new Uri(PatchUrl, "patchlist_always_win10.txt");
			VersionFileUrl     = new Uri(PatchUrl, "version.ver");
		}

		protected async Task PullManagementDataIfNull()
		{
			if (ManagementData is null)
			{
				await PullManagementData();
			}
		}

		public async Task<bool> IsInMaintenance()
		{
			await PullManagementDataIfNull();

			if (!ManagementData.TryGetValue("IsInMaintenance", out string value))
			{
				return false;
			}

			return value == "1";
		}


		public async Task<bool> IsGameUpToDate()
		{
			try
			{
				await PullManagementDataIfNull();

				if (ManagementData == null)
				{
					return true;
				}

				string localVersion = await Task.Run(() => File.Exists(VersionFilePath) ? File.ReadAllText(VersionFilePath) : string.Empty);

				using (var client = new AquaHttpClient())
				{
					client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
					string remoteVersion = await client.DownloadStringTaskAsync(VersionFileUrl);
					await Task.Run(() => File.WriteAllText(Path.Combine(GameConfigFolder, "_version.ver"), remoteVersion));
					return localVersion == remoteVersion;
				}
			}
			catch
			{
				return true;
			}
		}

		public async Task<bool> IsNewPrecedeAvailable()
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

		public async Task<Uri> BuildFileUri(PatchListSource source, string path)
		{
			await PullManagementDataIfNull();

			switch (source)
			{
				case PatchListSource.None:
					return null; // UNDONE

				case PatchListSource.Master:
					return new Uri(MasterUrl, path);

				case PatchListSource.Patch:
					return new Uri(PatchUrl, path);

				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, null);
			}
		}

		public async Task<Uri> BuildFileUri(PatchListEntry entry)
		{
			return await BuildFileUri(entry.Source, entry.Name);
		}
	}
}