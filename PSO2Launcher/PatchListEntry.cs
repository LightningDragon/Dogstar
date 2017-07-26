using System;
using System.Collections.Generic;

namespace Dogstar
{
	public class PatchListEntryComparer : IEqualityComparer<PatchListEntry>
	{
		public bool Equals(PatchListEntry x, PatchListEntry y) => x.Name == y.Name && x.Size == y.Size && x.Hash == y.Hash;

		public int GetHashCode(PatchListEntry obj) => obj.Name.GetHashCode() ^ obj.Size.GetHashCode() ^ obj.Hash.GetHashCode();
	}

	public enum PatchListSource
	{
		None,
		Master,
		Patch
	}

	public class PatchListEntry
	{
		public readonly string Name;
		public readonly long Size;
		public readonly string Hash;
		public readonly PatchListSource Source;
		private readonly string unknown;

		public PatchListEntry(string s)
		{
			string[] split = s.Split('\t');

			if (split.Length < 3)
			{
				throw new ArgumentOutOfRangeException();
			}

			if (split.Length <= 3)
			{
				Name = split[0];
				Size = long.Parse(split[1]);
				Hash = split[2];
			}
			else
			{
				Name = split[0];
				Hash = split[1];
				Size = long.Parse(split[2]);

				switch (split[3])
				{
					case "m":
						Source = PatchListSource.Master;
						break;

					case "p":
						Source = PatchListSource.Patch;
						break;
				}

				if (split.Length > 4)
				{
					unknown = split[4];
				}
			}

			if (string.IsNullOrEmpty(Name))
			{
				throw new NullReferenceException();
			}
		}
	}
}
