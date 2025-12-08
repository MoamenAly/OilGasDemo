namespace UIThemes.Samples
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Scripting;

	/// <summary>
	/// Sample of the extended theme.
	/// </summary>
	// [CreateAssetMenu(fileName = "UI Theme Extended", menuName = "UI Themes/Create ThemeExtended")]
	[Serializable]
	public class ThemeExtended : Theme
	{
		readonly List<string> disabledProperties = new List<string>()
		{
			nameof(Sprites),
		};

		/// <summary>
		/// Rotated sprites.
		/// </summary>
		[SerializeField]
		protected ValuesTable<RotatedSprite> RotatedSpritesTable = new ValuesTable<RotatedSprite>();

		/// <summary>
		/// Rotated sprites.
		/// </summary>
		[UIThemes.PropertyGroup(typeof(RotatedSpriteView), "UI Themes: Change Rotated Sprite")]
		public ValuesWrapper<RotatedSprite> RotatedSprites => new ValuesWrapper<RotatedSprite>(this, RotatedSpritesTable);

		/// <inheritdoc/>
		public override Type GetTargetType() => typeof(ThemeTargetExtended);

		/// <inheritdoc/>
		public override void Copy(Variation source, Variation destination)
		{
			base.Copy(source, destination);
			RotatedSpritesTable.Copy(source.Id, destination.Id);
		}

		/// <inheritdoc/>
		protected override void DeleteVariationValues(VariationId id)
		{
			base.DeleteVariationValues(id);
			RotatedSpritesTable.DeleteVariation(id);
		}

		/// <inheritdoc/>
		public override bool IsActiveProperty(string name) => !disabledProperties.Contains(name);

		/// <summary>
		/// Add properties.
		/// </summary>
		[PropertiesRegistry]
		[Preserve]
		public static void AddProperties()
		{
			PropertyWrappers<RotatedSprite>.Add(new RotatedSpriteWrapper());
		}
	}
}