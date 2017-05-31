using System;

namespace Teleopti.Ccc.Domain.Common
{
	public static class StringExtensions
	{
		public static bool IsAnUrl(this string potentialUrl)
		{
			return potentialUrl.StartsWith("http://") || potentialUrl.StartsWith("https://");
		}
	}

	public static class GuidExtension
	{
		public static Guid[] AsArray(this Guid g)
		{
			return new[] { g };
		}

		public static Guid?[] AsNullableArray(this Guid g)
		{
			return new Guid?[] { g };
		}
	}
}