namespace Dogstar.GameEditionManagement
{
	public class JapanEditionManager : GameEditionManager
	{
		/// <inheritdoc />
		public override EditionPathProvider PathProvider { get; protected set; } = new JapanPathProvider();
		/// <inheritdoc />
		public override EditionPatchListProvider PatchListProvider { get; protected set; } = new JapanPatchListProvider();
	}
}
