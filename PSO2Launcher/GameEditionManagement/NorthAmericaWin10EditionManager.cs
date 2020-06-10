namespace Dogstar.GameEditionManagement
{
	public class NorthAmericaWin10EditionManager : GameEditionManager
	{
		/// <inheritdoc />
		public override EditionPathProvider PathProvider { get; protected set; } = new NorthAmericaWin10PathProvider();
		/// <inheritdoc />
		public override EditionPatchListProvider PatchListProvider { get; protected set; } = new NorthAmericaWin10PatchListProvider();
	}
}
