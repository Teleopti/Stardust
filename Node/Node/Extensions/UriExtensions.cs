using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;

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

		public static async Task<HttpResponseMessage> PostAsync(this Uri uri,
		                                                        Uri apiEndpoint,
		                                                        CancellationToken cancellationToken)
		{
			Logger.DebugWithLineNumber("Start.");

			IHttpSender httpSender = new HttpSender();

			return await httpSender.PostAsync(apiEndpoint,
			                                  uri,
			                                  cancellationToken);
		}
	}
}