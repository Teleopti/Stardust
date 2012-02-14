using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Web.Core
{
	public static class CoreExtensions
	{
		public static IEnumerable<TResult> SelectOrEmpty<TSource, TResult>(this IEnumerable<TSource> collection, Func<TSource, TResult> query)
		{
			return collection == null ? new TResult[] {}: collection.Select(query);
		}

		public static bool AnyOrFalse<TSource>(this IEnumerable<TSource> collection)
		{
			return collection != null && collection.Any();
		}

		public static TSource SingleOrDefaultNullSafe<TSource>(this IEnumerable<TSource> collection)
		{
			return collection == null ? default(TSource) : collection.SingleOrDefault();
		}

		public static IEnumerable<T> MakeSureNotNull<T>(this IEnumerable<T> instance)
		{
			return instance ?? new T[] {};
		}
	}
}
