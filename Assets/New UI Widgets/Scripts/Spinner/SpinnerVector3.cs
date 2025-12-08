namespace UIWidgets
{
	using System;
	using UnityEngine;
	using UnityEngine.Events;

	/// <summary>
	/// Three Spinners combined to represent Vector3.
	/// </summary>
	public class SpinnerVector3 : MonoBehaviourConditional
	{
		/// <summary>
		/// Event.
		/// </summary>
		[Serializable]
		public class Vector3Event : UnityEvent<Vector3>
		{
		}

		[SerializeField]
		bool interactable = true;

		/// <summary>
		/// Interactable.
		/// </summary>
		public bool Interactable
		{
			get => interactable;

			set
			{
				interactable = value;

				SpinnerX.Interactable = interactable;
				SpinnerY.Interactable = Interactable;
				SpinnerZ.Interactable = Interactable;
			}
		}

		/// <summary>
		/// Current value.
		/// </summary>
		[SerializeField]
		protected Vector3 CurrentValue;

		/// <summary>
		/// Value.
		/// </summary>
		public Vector3 Value
		{
			get => new Vector3(SpinnerX.Value, SpinnerY.Value, SpinnerZ.Value);

			set
			{
				if (CurrentValue != value)
				{
					CurrentValue = value;
					SpinnerX.Value = value.x;
					SpinnerY.Value = value.y;
					SpinnerZ.Value = value.z;

					OnValueChanged.Invoke(CurrentValue);
				}
			}
		}

		/// <summary>
		/// Spinner for the Vector3.x;
		/// </summary>
		[SerializeField]
		protected SpinnerFloat SpinnerX;

		/// <summary>
		/// Spinner for the Vector3.y;
		/// </summary>
		[SerializeField]
		protected SpinnerFloat SpinnerY;

		/// <summary>
		/// Spinner for the Vector3.z;
		/// </summary>
		[SerializeField]
		protected SpinnerFloat SpinnerZ;

		/// <summary>
		/// Value changed event.
		/// </summary>
		[SerializeField]
		public Vector3Event OnValueChanged = new Vector3Event();

		/// <summary>
		/// Subscription on spinner value changed.
		/// </summary>
		protected Subscriptions<Subscription<float>> SpinnersValueChanged;

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

			UnityAction<float> callback = ValueChanged;

			SpinnersValueChanged = new Subscriptions<Subscription<float>>(
				new Subscription<float>(SpinnerX.onValueChangeFloat, callback),
				new Subscription<float>(SpinnerY.onValueChangeFloat, callback),
				new Subscription<float>(SpinnerZ.onValueChangeFloat, callback));

			Interactable = interactable;
		}

		/// <summary>
		/// Process value changed event.
		/// </summary>
		/// <param name="ignore">Argument is ignored.</param>
		protected virtual void ValueChanged(float ignore = 0f)
		{
			if (CurrentValue == Value)
			{
				return;
			}

			CurrentValue = Value;
			OnValueChanged.Invoke(CurrentValue);
		}
	}
}