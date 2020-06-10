using System;
using System.IO;

namespace Dogstar.GameEditionManagement
{
	class JapanPathProvider : EditionPathProvider
	{
		public override string GameConfigFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SEGA", "PHANTASYSTARONLINE2");
	}
}