using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Common
{
	public static class StringExtensions
	{
		public static bool IsAnUrl(this string potentialUrl)
		{
			return potentialUrl.StartsWith("http://") || potentialUrl.StartsWith("https://");
		}
	}

	public static class ThingExtensions
	{
		public static T[] AsArray<T>(this T g)
		{
			return new[] { g };
		}
		
		public static IEnumerable<T> AsIEnumerable<T>(this T g)
		{
			yield return g;
		}
	}
	
	public static class GuidExtension
	{
		
		public static Guid?[] AsNullableArray(this Guid g)
		{
			return new Guid?[] { g };
		}
	}
}