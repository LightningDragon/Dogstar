using System;

namespace Dogstar.GameEditionManagement
{
	class JapanPatchListProvider : EditionPatchListProvider
	{
		public override Uri BasePrecede => new Uri("http://download.pso2.jp/patch_prod/patches_precede/");
		public override Uri ManagementUrl => new Uri("http://patch01.pso2gs.net/patch_prod/patches/management_beta.txt");
	}
}