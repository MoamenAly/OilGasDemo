namespace UIWidgets.Examples
{
	using UIWidgets;
	using UnityEngine;

	/// <summary>
	/// Linear GroupedTileView.
	/// </summary>
	public class LinearGroupedTileView : ListViewCustom<GroupedTileViewComponentBase, Photo>
	{
		/// <summary>
		/// Real DataSource (use instead of DataSource).
		/// </summary>
		public ObservableList<Photo> RealDataSource = new ObservableList<Photo>();

		/// <summary>
		/// Grouped data.
		/// </summary>
		public LinearGroupedList<Photo> GroupedData = new LinearGroupedList<Photo>(x => x.IsGroup);

		/// <summary>
		/// Header template.
		/// </summary>
		[SerializeField]
		protected GroupedTileViewComponentHeader HeaderTemplate;

		/// <summary>
		/// Empty header template.
		/// </summary>
		[SerializeField]
		protected GroupedTileViewComponentEmpty HeaderEmptyTemplate;

		/// <summary>
		/// Item template.
		/// </summary>
		[SerializeField]
		protected GroupedTileViewComponentItem ItemTemplate;

		/// <summary>
		/// Empty item template;
		/// </summary>
		[SerializeField]
		protected GroupedTileViewComponentEmpty ItemEmptyTemplate;

		class Selector : IListViewTemplateSelector<GroupedTileViewComponentBase, Photo>
		{
			readonly GroupedTileViewComponentBase headerTemplate;

			readonly GroupedTileViewComponentBase headerEmptyTemplate;

			readonly GroupedTileViewComponentBase itemTemplate;

			readonly GroupedTileViewComponentBase itemEmptyTemplate;

			readonly GroupedTileViewComponentBase[] templates;

			public Selector(
				GroupedTileViewComponentHeader headerTemplate,
				GroupedTileViewComponentEmpty headerEmptyTemplate,
				GroupedTileViewComponentItem itemTemplate,
				GroupedTileViewComponentEmpty itemEmptyTemplate)
			{
				this.headerTemplate = headerTemplate;
				this.headerEmptyTemplate = headerEmptyTemplate;
				this.itemTemplate = itemTemplate;
				this.itemEmptyTemplate = itemEmptyTemplate;

				templates = new[] { this.headerTemplate, this.headerEmptyTemplate, this.itemTemplate, this.itemEmptyTemplate, };
			}

			public GroupedTileViewComponentBase[] AllTemplates() => templates;

			public GroupedTileViewComponentBase Select(int index, Photo item)
			{
				if (item.IsGroup)
				{
					return item.IsEmpty ? headerEmptyTemplate : headerTemplate;
				}
				else
				{
					return item.IsEmpty ? itemEmptyTemplate : itemTemplate;
				}
			}
		}

		bool isGroupedListViewInited;

		/// <inheritdoc/>
		public override void Init()
		{
			if (isGroupedListViewInited)
			{
				return;
			}

			isGroupedListViewInited = true;

			base.Init();

			GroupedData.EmptyHeaderItem = new Photo() { IsGroup = true, IsEmpty = true };
			GroupedData.EmptyItem = new Photo() { IsEmpty = true };
			GroupedData.ItemsPerBlock = ListRenderer.GetItemsPerBlock();

			GroupedData.Input = RealDataSource;
			GroupedData.Output = DataSource;
		}

		/// <inheritdoc/>
		protected override IListViewTemplateSelector<GroupedTileViewComponentBase, Photo> CreateTemplateSelector()
		{
			return new Selector(HeaderTemplate, HeaderEmptyTemplate, ItemTemplate, ItemEmptyTemplate);
		}

		/// <inheritdoc/>
		public override void UpdateItems()
		{
			base.UpdateItems();

			GroupedData.ItemsPerBlock = ListRenderer.GetItemsPerBlock();
		}

		/// <inheritdoc/>
		public override void Resize()
		{
			base.Resize();

			GroupedData.ItemsPerBlock = ListRenderer.GetItemsPerBlock();
		}
	}
}