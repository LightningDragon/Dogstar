using System;
using System.IO;

namespace Dogstar
{
	class NorthAmericaPatchProvider : PatchProvider
	{
		public override Uri BasePrecede => new Uri("http://download.pso2.com/patch_prod/patches_precede/");

		public override Uri ManagementUrl => new Uri("http://patch01.pso2.com/patch_prod/patches/management_beta.txt");

		public override string GameConfigFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SEGA", "PHANTASYSTARONLINE2_NA");

		public override Uri PatchListAlwaysUrl => new Uri(PatchListUrl, "patchlist_always_win10.txt");
	}
}
