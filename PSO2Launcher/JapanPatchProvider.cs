using System;
using System.IO;

namespace Dogstar
{
	class JapanPatchProvider : PatchProvider
	{
		public override Uri BasePrecede => new Uri("http://download.pso2.jp/patch_prod/patches_precede/");

		public override Uri ManagementUrl => new Uri("http://patch01.pso2gs.net/patch_prod/patches/management_beta.txt");

		public override string GameConfigFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SEGA", "PHANTASYSTARONLINE2");
	}
}