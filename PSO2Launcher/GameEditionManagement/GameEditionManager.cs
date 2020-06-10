using System.IO;
using System.Threading.Tasks;

namespace Dogstar.GameEditionManagement
{
	public abstract class GameEditionManager
	{
		public abstract EditionPathProvider      PathProvider      { get; protected set; }
		public abstract EditionPatchListProvider PatchListProvider { get; protected set; }
		public PsoSettings GameSettings { get; }

		protected GameEditionManager()
		{
			GameSettings = new PsoSettings(this);
		}

		public async Task<bool> IsGameUpToDate()
		{
			if (!File.Exists(PathProvider.VersionFilePath))
			{
				return false;
			}

			string localVersion = await Task.Run(() => File.ReadAllText(PathProvider.VersionFilePath));
			string remoteVersion;

			try
			{
				remoteVersion = await PatchListProvider.GetRemoteVersion();
			}
			catch
			{
				// HACK: we should NOT be ignoring ALL exceptions like this!
				return true;
			}

			return localVersion == remoteVersion;
		}

		public async Task<bool> IsNewPrecedeAvailable()
		{
			await PatchListProvider.PullManagementData();

			if (PatchListProvider.ManagementData.ContainsKey("PrecedeVersion") && PatchListProvider.ManagementData.ContainsKey("PrecedeCurrent"))
			{
				var version = PatchListProvider.ManagementData["PrecedeVersion"];
				var listnum = PatchListProvider.ManagementData["PrecedeCurrent"];
				var current = await Task.Run(() => File.Exists(PathProvider.PrecedeTxtPath) ? File.ReadAllText(PathProvider.PrecedeTxtPath) : string.Empty);
				return string.IsNullOrEmpty(current) || current != $"{version}\t{listnum}";
			}

			return false;
		}

		public async Task<bool> IsInMaintenance()
		{
			await PatchListProvider.PullManagementDataIfNull();

			if (!PatchListProvider.ManagementData.TryGetValue("IsInMaintenance", out string value))
			{
				return false;
			}

			return value == "1";
		}
	}
}