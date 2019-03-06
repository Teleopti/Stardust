using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Collection
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
		private static readonly Random _random = new Random((int) DateTime.Now.TimeOfDay.TotalSeconds);


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
			InParameter.ValueMustBeLargerThanZero(nameof(batchSize), batchSize);
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


		//rk: testet from specification test
		public static IEnumerable<T> FilterBySpecification<T>(this IEnumerable<T> source, ISpecification<T> specification)
		{
			var list = new List<T>();
			foreach (var item in source)
			{
				if (specification.IsSatisfiedBy(item))
					list.Add(item);
			}

			return list;
		}


		/// <summary>
		/// Gets RandomElements from IEnumerable
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">The list.</param>
		/// <param name="numberOfElements">The number of elements.</param>
		/// <param name="onlyUnique">if set to <c>true</c> [only unique].</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2008-04-18
		/// </remarks>
		public static IEnumerable<T> GetRandom<T>(this IEnumerable<T> list, int numberOfElements, bool onlyUnique)
		{
			InParameter.NotNull(nameof(list), list);
			return RandomIterator(list, numberOfElements, onlyUnique);
		}


		private static IEnumerable<T> RandomIterator<T>(IEnumerable<T> list, int numberOfElements, bool onlyUnique)
		{
			List<T> buffer = new List<T>(list);
			numberOfElements = Math.Min(numberOfElements, buffer.Count);

			if (numberOfElements > 0)
			{
				for (int i = 0; i < numberOfElements; i++)
				{
					int randomIndex = _random.Next(buffer.Count);
					yield return buffer[randomIndex];
					if (onlyUnique) buffer.RemoveAt(randomIndex);
				}
			}
		}

		public static T GetRandom<T>(this IEnumerable<T> source)
		{
			return source.ElementAt(_random.Next(0, source.Count()));
		}

		public static bool IsEmpty<T>(this IEnumerable<T> source)
		{
			return !source.Any();
		}

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
		{
			return source == null || !source.Any();
		}

		public static IEnumerable<T> CopyEnumerable<T>(this IEnumerable source)
		{
			return source.Cast<T>().ToList();
		}

		public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
		{
			var rnd = new Random();
			return source.OrderBy(_ => rnd.Next());
		}

		public static IEnumerable<T> RandomizeBetter<T>(this IEnumerable<T> source)
		{
			var rnd = new Random(Guid.NewGuid().GetHashCode());
			return source.OrderBy(_ => rnd.Next());
		}

		public static bool NonSequenceEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
		{
			var cnt = new Dictionary<T, int>();
			foreach (T s in source)
			{
				if (cnt.ContainsKey(s))
				{
					cnt[s]++;
				}
				else
				{
					cnt.Add(s, 1);
				}
			}

			foreach (T s in other)
			{
				if (cnt.ContainsKey(s))
				{
					cnt[s]--;
				}
				else
				{
					return false;
				}
			}

			return cnt.Values.All(c => c == 0);
		}

		public static IEnumerable<T> TransitionsOf<T, T2>(this IEnumerable<T> source, Func<T, T2> value)
		{
			if (!source.Any())
				yield break;

			yield return source.First();
			T previousItem = source.First();
			foreach (var item in source.Skip(1))
			{
				if (!EqualityComparer<T2>.Default.Equals(value(previousItem), value(item)))
				{
					yield return item;
					previousItem = item;
				}
			}
		}

		public static T Second<T>(this IEnumerable<T> source)
		{
			return source.ElementAt(1);
		}

		public static T Third<T>(this IEnumerable<T> source)
		{
			return source.ElementAt(2);
		}
		
		public static IEnumerable<T> Except<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			return source.Where(x => !predicate(x));
		}

		public static IEnumerable<T> Infinite<T>(this IEnumerable<T> source)
		{
			while (true)
			{
				foreach (var a in source)
					yield return a;
			}
		}

		public static int IndexOf<T>(this T[] collection, T targetValue)
		{
			return Array.IndexOf(collection, targetValue);
		}

		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> instance)
		{
			return instance ?? Enumerable.Empty<T>();
		}

		public static TSource SingleOrDefaultNullSafe<TSource>(this IEnumerable<TSource> collection)
		{
			return collection == null ? default(TSource) : collection.SingleOrDefault();
		}
	}
}