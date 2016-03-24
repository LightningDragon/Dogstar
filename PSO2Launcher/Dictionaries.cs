using System.Collections.Generic;

namespace DogStar
{
	public class Dictionaries
	{
		public static Dictionary<string, string> GetTheme() => new Dictionary<string, string> { { "Dark", "BaseDark" }, { "Light", "BaseLight" } };

		public static Dictionary<string, string> GetColor()
		{
			return new Dictionary<string, string>
			  {
				{"Blue", "blue"},
				{"Red", "red"},
				{"Green", "green"},
				{"Purple", "purple"},
				{"Orange", "orange"},
				{"Lime", "lime"},
				{"Emerald", "emerald"},
				{"Teal", "teal"},
				{"Cyan", "cyan"},
				{"Cobalt", "cobalt"},
				{"Indigo", "indigo"},
				{"Violet", "violet"},
				{"Pink", "pink"},
				{"Magenta", "magenta"},
				{"Crimson", "crimson"},
				{"Amber", "amber"},
				{"Yellow", "yellow"},
				{"Brown", "brown"},
				{"Olive", "olive"},
				{"Steel", "steel"},
				{"Mauve", "mauve"},
				{"Taupe", "taupe"},
				{"Sienna", "sienna"}
			  };
		}
	}
}