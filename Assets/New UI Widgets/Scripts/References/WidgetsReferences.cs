#if UNITY_EDITOR
namespace UIWidgets
{
	using System;
	using System.IO;
	using UnityEditor;
	using UnityEngine;
#if UIWIDGETS_TMPRO_SUPPORT && UIWIDGETS_TMPRO_4_0_OR_NEWER
	using FontAsset = UnityEngine.TextCore.Text.FontAsset;
#elif UIWIDGETS_TMPRO_SUPPORT
	using FontAsset = TMPro.TMP_FontAsset;
#else
	using FontAsset = UnityEngine.ScriptableObject;
#endif

	/// <summary>
	/// Themes references.
	/// </summary>
	[Serializable]
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/project-settings.html")]
	public class WidgetsReferences : ScriptableObject
	{
		[SerializeField]
		bool instantiatePrefabs;

		/// <summary>
		/// Instantiate prefabs.
		/// </summary>
		public bool InstantiatePrefabs
		{
			get => instantiatePrefabs;

			set
			{
				if (instantiatePrefabs != value)
				{
					instantiatePrefabs = value;
					EditorUtility.SetDirty(this);
				}
			}
		}

		[SerializeField]
		bool attachTheme = true;

		/// <summary>
		/// Attach default to the widgets created from the menu.
		/// </summary>
		public bool AttachTheme
		{
			get => attachTheme;

			set
			{
				if (attachTheme != value)
				{
					attachTheme = value;
					EditorUtility.SetDirty(this);
				}
			}
		}

		[SerializeField]
		PrefabsMenu current;

		/// <summary>
		/// Current theme.
		/// </summary>
		public PrefabsMenu Current
		{
			get
			{
				return current;
			}

			set
			{
				if (!ReferenceEquals(current, value))
				{
					current = value;
					EditorUtility.SetDirty(this);
				}
			}
		}

		[SerializeField]
		FontAsset defaultFont;

		/// <summary>
		/// Default font.
		/// </summary>
		public FontAsset DefaultFont
		{
			get
			{
				return defaultFont;
			}

			set
			{
				if (!ReferenceEquals(defaultFont, value))
				{
					defaultFont = value;
					EditorUtility.SetDirty(this);
				}
			}
		}

		/// <summary>
		/// Widgets references.
		/// </summary>
		public static WidgetsReferences Instance
		{
			get
			{
				var refs = Resources.FindObjectsOfTypeAll<WidgetsReferences>();
				if (refs.Length > 0)
				{
					return refs[0];
				}

				var guids = AssetDatabase.FindAssets("t:WidgetsReferences");
				if (guids.Length > 0)
				{
					var path = AssetDatabase.GUIDToAssetPath(guids[0]);
					return AssetDatabase.LoadAssetAtPath<WidgetsReferences>(path);
				}

				return Create();
			}
		}

		/// <summary>
		/// Create references.
		/// </summary>
		/// <returns>Created instance.</returns>
		static WidgetsReferences Create()
		{
			var folder = ReferenceGUID.EditorFolder;
			if (string.IsNullOrEmpty(folder))
			{
				return null;
			}

			var path = folder + Path.DirectorySeparatorChar + "Widgets References.asset";
			return UtilitiesEditor.CreateScriptableObjectAsset<WidgetsReferences>(path);
		}
	}
}
#endif