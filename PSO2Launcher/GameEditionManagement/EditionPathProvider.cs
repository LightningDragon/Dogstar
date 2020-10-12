using System;
using System.IO;
using Dogstar.Properties;

namespace Dogstar.GameEditionManagement
{
	public abstract class EditionPathProvider
	{
		public abstract string GameConfigFolder { get; }

		// TODO: write some good docs for this - pso2launcher puts .pat files for the .exe and .dll files in here as well as launcherlist.txt and patchlist_always
		public virtual string GameDownloadFolder => Path.Combine(GameConfigFolder, "download");

		public virtual string LauncherListPath      => Path.Combine(GameConfigFolder,   "launcherlist.txt");
		public virtual string PatchListPath         => Path.Combine(GameConfigFolder,   "_patchlist.txt");
		public virtual string PatchListAlwaysPath   => Path.Combine(GameConfigFolder,   "_patchlist_always.txt");
		public virtual string VersionFilePath       => Path.Combine(GameConfigFolder,   "version.ver");
		public virtual string PrecedeTxtPath        => Path.Combine(GameConfigFolder,   "precede.txt");
		public virtual string ConfigurationFilePath => Path.Combine(GameConfigFolder,   "user.pso2");

		// UNDONE: PSO2NA has a win32_na folder!!! this is used for precede patches (bad!) and preemptive directory creation (also bad!)
		// UNDONE: make Settings.Default.GameFolder selectable for region
		[Obsolete("The data folder path should not be directly accessed.")]
		public virtual string DataFolder => Path.Combine(Settings.Default.GameFolder, "data", "win32");

		// UNDONE: make Settings.Default.GameFolder selectable for region
		public virtual string PrecedeFolder => Path.Combine(Settings.Default.GameFolder, "_precede");
	}
}
