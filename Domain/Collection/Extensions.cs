using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

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
			InParameter.ValueMustBeLargerThanZero("batchSize", batchSize);
			var wrappedSource = new List<T>(source);
			var count = (int) Math.Ceiling(wrappedSource.Count/(double) batchSize);
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
			InParameter.NotNull("list", list);
			return RandomIterator(list, numberOfElements, onlyUnique);
		}

		public static IEnumerable<T> RandomIterator<T>(this IEnumerable<T> list)
		{
			InParameter.NotNull("list", list);
			return RandomIterator(list, list.Count(), true);
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
			Random rnd = new Random();
			return source.OrderBy((item) => rnd.Next());
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

		public static IEnumerable<Pair<T>> WithPrevious<T>(this IEnumerable<T> source)
		{
			var previous = default(T);
			foreach (var @this in source)
			{
				if (!EqualityComparer<T>.Default.Equals(previous, default(T)))
				{
					yield return new Pair<T>
					{
						This = @this,
						Previous = previous
					};
				}
				previous = @this;
			}
		}

		public class Pair<T>
		{
			public T This { get; set; }
			public T Previous { get; set; }
		}

		public static IEnumerable<T> TransitionsOf<T, T2>(this IEnumerable<T> source, Func<T, T2> value)
		{
			return source
				.Take(1)
				.Concat(
					source
						.WithPrevious()
						.Where(x => !value(x.This).Equals(value(x.Previous)))
						.Select(x => x.This)
				);
		}

		public static IEnumerable<T> ExceptLast<T>(this IEnumerable<T> source)
		{
			using (var e = source.GetEnumerator())
			{
				if (e.MoveNext())
				{
					for (var value = e.Current; e.MoveNext(); value = e.Current)
					{
						yield return value;
					}
				}
			}
		}

		public static T Second<T>(this IEnumerable<T> source)
		{
			return source.ElementAt(1);
		}

		public static IEnumerable<T> Except<T>(this IEnumerable<T> source, Func<T, bool> predicate)
		{
			return source.Where(x => !predicate(x));
		}

		public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
		{
			foreach (var a in source)
				yield return a;
			yield return item;
		}

		public static int IndexOf<T>(this IEnumerable<T> collection, T targetValue)
		{
			return collection.Select((value, index) => new { value, index })
						.Where(pair => pair.value.Equals(targetValue))
						.Select(pair => pair.index + 1)
						.FirstOrDefault() - 1;
		}

		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> instance)
		{
			return instance ?? Enumerable.Empty<T>();
		}
	}
}
