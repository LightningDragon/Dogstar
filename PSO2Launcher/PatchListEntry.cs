using System;
using System.Collections.Generic;

namespace Dogstar
{
	public class PatchListEntryComparer : IEqualityComparer<PatchListEntry>
	{
		public bool Equals(PatchListEntry x, PatchListEntry y) => x.Name == y.Name && x.Size == y.Size && x.Hash == y.Hash;

		public int GetHashCode(PatchListEntry obj) => obj.Name.GetHashCode() ^ obj.Size.GetHashCode() ^ obj.Hash.GetHashCode();
	}

	public class PatchListEntry
	{
		public string Name;
		public long Size;
		public string Hash;

		public PatchListEntry(string name, string size, string hash) : this(name, Convert.ToInt64(size), hash)
		{
		}

		public PatchListEntry(string name, long size, string hash)
		{
			Name = name;
			Size = size;
			Hash = hash;
		}
	}
}
