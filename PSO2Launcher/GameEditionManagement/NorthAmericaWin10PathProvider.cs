using System;
using System.IO;

namespace Dogstar.GameEditionManagement
{
	class NorthAmericaWin10PathProvider : EditionPathProvider
	{
		public override string GameConfigFolder    => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SEGA", "PHANTASYSTARONLINE2_NA");
		public override string PatchListAlwaysPath => Path.Combine(GameConfigFolder,                                                 "_patchlist_always_win10.txt");
	}
}