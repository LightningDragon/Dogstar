using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dogstar
{
	public static class PluginSetting
	{
		public static IEnumerable<dynamic> GetShit()
		{
			yield return new {Name = "Test", Version = 1.0};
		}

		public static void Add(string str)
		{

		}
	}
}
