namespace UIWidgets.Examples
{
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// Draggable + Resizable + Rotatable.
	/// </summary>
	[RequireComponent(typeof(Draggable))]
	[RequireComponent(typeof(Resizable))]
	[RequireComponent(typeof(Rotatable))]
	public class DraggableResizableRotatable : MonoBehaviour
	{
		/// <summary>
		/// Show handles only if Target (or any handle) selected.
		/// Target and Handles should have Selectable component.
		/// </summary>
		[SerializeField]
		[Tooltip("Target and Handles should have Selectable component.")]
		protected bool ShowHandlesOnSelect = false;

		/// <summary>
		/// Draggable.
		/// </summary>
		protected Draggable Draggable;

		/// <summary>
		/// Resizable.
		/// </summary>
		protected Resizable Resizable;

		/// <summary>
		/// Resizable handles.
		/// </summary>
		protected ResizableHandles ResizableHandles;

		/// <summary>
		/// Rotatable.
		/// </summary>
		protected Rotatable Rotatable;

		/// <summary>
		/// Rotatable handles.
		/// </summary>
		protected RotatableHandle RotatableHandle;

		bool isInited;

		/// <summary>
		/// Process the start event.
		/// </summary>
		protected virtual void Start()
		{
			Init();
		}

		/// <summary>
		/// Init this instance.
		/// </summary>
		public virtual void Init()
		{
			if (isInited)
			{
				return;
			}

			isInited = true;

			Draggable = GetComponent<Draggable>();
			Draggable.Init();

			Resizable = GetComponent<Resizable>();
			Resizable.Init();

			ResizableHandles = GetComponent<ResizableHandles>();
			ResizableHandles.Init();
			ResizableHandles.HandlesState = ResizableHandlesState;

			Rotatable = GetComponent<Rotatable>();
			Rotatable.Init();

			RotatableHandle = GetComponent<RotatableHandle>();
			RotatableHandle.Init();
			RotatableHandle.HandleState = RotatableHandleState;
		}

		/// <summary>
		/// Process the destroy event.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (ResizableHandles != null)
			{
				ResizableHandles.HandlesState = null;
			}

			if (RotatableHandle != null)
			{
				RotatableHandle.HandleState = null;
			}
		}

		/// <summary>
		/// Get handles visibility.
		/// </summary>
		/// <param name="resizableHandles">Resizable handles.</param>
		/// <param name="eventData">Event data.</param>
		/// <param name="select">Select.</param>
		/// <returns>true to show handles; otherwise false.</returns>
		protected virtual bool ResizableHandlesState(ResizableHandles resizableHandles, BaseEventData eventData, bool select)
		{
			var state = HandlesState(eventData, select);
			RotatableHandle.ToggleHandle(state);

			return state;
		}

		/// <summary>
		/// Get handles visibility.
		/// </summary>
		/// <param name="rotatableHandle">Rotatable handle.</param>
		/// <param name="eventData">Event data.</param>
		/// <param name="select">Select or deselect event.</param>
		/// <returns>true to show handles; otherwise false.</returns>
		protected virtual bool RotatableHandleState(RotatableHandle rotatableHandle, BaseEventData eventData, bool select)
		{
			var state = HandlesState(eventData, select);
			ResizableHandles.ToggleHandles(state);

			return state;
		}

		/// <summary>
		/// Get handles visibility.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		/// <param name="select">Select or deselect event.</param>
		/// <returns>true to show handles; otherwise false.</returns>
		protected virtual bool HandlesState(BaseEventData eventData, bool select)
		{
			if (!ShowHandlesOnSelect)
			{
				return true;
			}

			if (select)
			{
				return true;
			}

			if (!(eventData is PointerEventData ev))
			{
				return false;
			}

			return ResizableHandles.IsControlled(ev.pointerEnter) || RotatableHandle.IsControlled(ev.pointerEnter);
		}

		/// <summary>
		/// Set target.
		/// </summary>
		/// <param name="target">Target.</param>
		public void SetTarget(RectTransform target)
		{
			gameObject.SetActive(true);

			Draggable.Target = target;

			Resizable.Target = target;
			ResizableHandles.ToggleHandles(true);

			Rotatable.Target = target;
			RotatableHandle.ToggleHandle(true);
		}
	}
}