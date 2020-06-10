using System.IO;
using Dogstar.Properties;

namespace Dogstar.GameEditionManagement
{
	public abstract class EditionPathProvider
	{
		public abstract string GameConfigFolder { get; }

		public virtual string LauncherListPath      => Path.Combine(GameConfigFolder, "launcherlist.txt");
		public virtual string PatchListPath         => Path.Combine(GameConfigFolder, "_patchlist.txt");
		public virtual string PatchListAlwaysPath   => Path.Combine(GameConfigFolder, "_patchlist_always.txt");
		public virtual string VersionFilePath       => Path.Combine(GameConfigFolder, "version.ver");
		public virtual string PrecedeTxtPath        => Path.Combine(GameConfigFolder, "precede.txt");
		public virtual string ConfigurationFilePath => Path.Combine(GameConfigFolder, "user.pso2");

		// UNDONE: make Settings.Default.GameFolder selectable for region
		public static string DataFolder => Path.Combine(Settings.Default.GameFolder, "data", "win32");

		// UNDONE: make Settings.Default.GameFolder selectable for region
		public static string PrecedeFolder => Path.Combine(Settings.Default.GameFolder, "_precede");
	}
}
