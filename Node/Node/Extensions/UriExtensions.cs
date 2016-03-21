using System;
using log4net;

namespace Stardust.Node.Extensions
{
	public static class UriExtensions
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (UriExtensions));

		public static void ThrowArgumentNullExceptionWhenNull(this Uri uri)
		{
			if (uri.IsNull())
			{
				throw new ArgumentNullException();
			}
		}

		public static void ThrowArgumentExceptionWhenNull(this Uri uri)
		{
			if (uri.IsNull())
			{
				throw new ArgumentException();
			}
		}

		public static bool IsValid(this Uri uri)
		{
			uri.ThrowArgumentExceptionWhenNull();

			return uri.Host.HasValue();
		}

		public static bool IsNull(this Uri uri)
		{
			return uri == null;
		}

		public static bool IsNotNull(this Uri uri)
		{
			return uri != null;
		}
	}
}