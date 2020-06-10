using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Threading.Tasks;

namespace Dogstar.GameEditionManagement
{
	// TODO: rename - it does more than provide patch information
	public abstract class EditionPatchListProvider
	{
		public Dictionary<string, string> ManagementData { get; private set; }

		public abstract Uri BasePrecede   { get; }
		public abstract Uri ManagementUrl { get; }

		public Uri MasterUrl { get; private set; }
		public Uri PatchUrl  { get; private set; }

		public virtual Uri LauncherListUrl    => new Uri(PatchUrl, "launcherlist.txt");
		public virtual Uri PatchListUrl       => new Uri(PatchUrl, "patchlist.txt");
		public virtual Uri PatchListAlwaysUrl => new Uri(PatchUrl, "patchlist_always.txt");
		public virtual Uri VersionFileUrl     => new Uri(PatchUrl, "version.ver");

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
		}

		public async Task PullManagementDataIfNull()
		{
			if (ManagementData is null)
			{
				await PullManagementData();
			}
		}

		public async Task<string> GetRemoteVersion()
		{
			await PullManagementDataIfNull();

			if (ManagementData == null)
			{
				return string.Empty;
			}

			using (var client = new AquaHttpClient())
			{
				client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
				return await client.DownloadStringTaskAsync(VersionFileUrl);
			}
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