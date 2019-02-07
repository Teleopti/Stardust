using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Collection2
{
	/// <summary>
	/// ExtensionMethods for collections
	/// </summary>
	/// <remarks>
	/// Created by: henrika
	/// Created date: 2008-04-18
	/// </remarks>
	public static class Extensions
	{
		/// <summary>
		/// Runs the delagate act for every item in the list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <param name="act">The act.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-09-15
		/// </remarks>
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> act)
		{
			foreach (T item in source)
			{
				act(item);
			}

			return source;
		}

		/// <summary>
		/// Batches the specified IEnumerable into a collection of IEnumerable base on the set batchSize.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <param name="batchSize">Size of the batch.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 1/28/2010
		/// </remarks>
		public static IEnumerable<T>[] Batch<T>(this IEnumerable<T> source, int batchSize)
		{
			//InParameter.ValueMustBeLargerThanZero(nameof(batchSize), batchSize);
			var wrappedSource = new List<T>(source);
			var count = (int) Math.Ceiling(wrappedSource.Count / (double) batchSize);
			var partitions = new List<T>[count];

			var k = 0;
			for (var i = 0; i < partitions.Length; i++)
			{
				partitions[i] = new List<T>(batchSize);
				for (var j = k; j < k + batchSize; j++)
				{
					if (j >= wrappedSource.Count)
						break;
					partitions[i].Add(wrappedSource[j]);
				}

				k += batchSize;
			}

			return partitions;
		}

		public static bool IsEmpty<T>(this IEnumerable<T> source)
		{
			return !source.Any();
		}
	}
}