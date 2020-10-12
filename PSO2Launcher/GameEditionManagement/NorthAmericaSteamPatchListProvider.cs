using System;

namespace Dogstar.GameEditionManagement
{
	class NorthAmericaSteamPatchListProvider : EditionPatchListProvider
	{
		public override Uri BasePrecede => new Uri("http://download.pso2.com/patch_prod/patches_precede/");

		public override Uri ManagementUrl => new Uri("http://patch01.pso2.com/patch_prod/patches/management_beta.txt");

		public override Uri PatchListAlwaysUrl => new Uri(PatchListUrl, "patchlist_always_steam.txt");
	}
}