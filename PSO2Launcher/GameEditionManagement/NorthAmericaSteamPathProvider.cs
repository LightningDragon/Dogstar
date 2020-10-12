using System;
using System.IO;

namespace Dogstar.GameEditionManagement
{
	class NorthAmericaSteamPathProvider : EditionPathProvider
	{
		public override string GameConfigFolder    => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SEGA", "PHANTASYSTARONLINE2_NA_STEAM");
		public override string PatchListAlwaysPath => Path.Combine(GameConfigFolder, "_patchlist_always_steam.txt");
	}
}