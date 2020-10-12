namespace Dogstar.GameEditionManagement
{
	public class NorthAmericaSteamEditionManager : GameEditionManager
	{
		/// <inheritdoc />
		public override EditionPathProvider PathProvider { get; protected set; } = new NorthAmericaSteamPathProvider();
		/// <inheritdoc />
		public override EditionPatchListProvider PatchListProvider { get; protected set; } = new NorthAmericaSteamPatchListProvider();

		// TODO: override LaunchGame to add "-optimize" flag
	}
}
