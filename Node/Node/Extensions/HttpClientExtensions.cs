using System;
using System.Net.Http;

namespace Stardust.Node.Extensions
{
	public static class HttpClientExtensions
	{
		public static bool IsNull(this HttpClient httpClient)
		{
			return httpClient == null;
		}

		public static void ThrowArgumentExceptionWhenNull(this HttpClient httpClient)
		{
			if (httpClient.IsNull())
			{
				throw new ArgumentException();
			}
		}
	}
}