namespace UIWidgets
{
	using System;
	using System.Collections.Generic;
	using UIWidgets.Pool;

	/// <summary>
	/// Stable sort helper.
	/// </summary>
	public static class StableSort
	{
		/// <summary>
		/// Item wrapper.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		public readonly struct Item<T>
		{
			/// <summary>
			/// Index.
			/// </summary>
			public readonly int Index;

			/// <summary>
			/// Data.
			/// </summary>
			public readonly T Data;

			/// <summary>
			/// Initializes a new instance of the <see cref="Item{T}"/> struct.
			/// </summary>
			/// <param name="index">Index.</param>
			/// <param name="data">Data.</param>
			public Item(int index, T data)
			{
				Index = index;
				Data = data;
			}
		}

		/// <summary>
		/// Sort.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <param name="items">Items.</param>
		/// <param name="comparison">Comparison.</param>
		/// <param name="reverse">Reverse sort.</param>
		public static void Sort<T>(IList<T> items, Comparison<T> comparison, bool reverse = false)
		{
			using var _ = ListPool<Item<T>>.Get(out var temp);

			for (var i = 0; i < items.Count; i++)
			{
				temp.Add(new Item<T>(i, items[i]));
			}

			var k = reverse ? -1 : 1;

			temp.Sort((a, b) =>
			{
				var result = comparison(a.Data, b.Data) * k;
				return result == 0 ? a.Index.CompareTo(b.Index) * k : result;
			});

			for (var i = 0; i < items.Count; i++)
			{
				items[i] = temp[i].Data;
			}
		}

		/// <summary>
		/// Sort.
		/// </summary>
		/// <typeparam name="T">Type of data.</typeparam>
		/// <param name="items">Items.</param>
		/// <param name="comparison">Comparison.</param>
		public static void Sort<T>(IList<T> items, Comparison<Item<T>> comparison)
		{
			using var _ = ListPool<Item<T>>.Get(out var temp);

			for (var i = 0; i < items.Count; i++)
			{
				temp.Add(new Item<T>(i, items[i]));
			}

			temp.Sort(comparison);

			for (var i = 0; i < items.Count; i++)
			{
				items[i] = temp[i].Data;
			}
		}
	}
}