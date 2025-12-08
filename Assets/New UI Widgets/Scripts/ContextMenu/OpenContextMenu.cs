namespace UIWidgets.Menu
{
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// Opens context menu by clicking on a non-UI game object.
	/// Requires PhysicsRaycaster on main camera for the 3D objects.
	/// Requires PhysicsRaycaster2D on main camera for the 2D objects.
	/// </summary>
	[HelpURL("https://ilih.name/unity-assets/UIWidgets/docs/components/open-contextmenu.html")]
	public class OpenContextMenu : MonoBehaviour, IPointerClickHandler
	{
		/// <summary>
		/// Context menu to open.
		/// </summary>
		[SerializeField]
		public ContextMenu Menu;

		/// <summary>
		/// Process the pointer click event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				Menu.Open(eventData);
			}
		}
	}
}