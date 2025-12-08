namespace UIWidgets.Examples
{
	using UIWidgets;

	/// <summary>
	/// GroupedTileView
	/// </summary>
	public abstract class GroupedTileViewBase : ListViewCustom<GroupedTileViewComponentBase, Photo>
	{
		/// <summary>
		/// Grouped data.
		/// </summary>
		public GroupedPhotos GroupedData = new GroupedPhotos();
	}
}