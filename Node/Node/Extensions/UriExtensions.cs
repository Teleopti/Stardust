using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Stardust.Node.Constants;
using Stardust.Node.Helpers;

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
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

			//----------------------------------------------
			// Call API.
			//----------------------------------------------
			using (var client = new HttpClient())
			{
				client.DefineDefaultRequestHeaders();

				var sez = JsonConvert.SerializeObject(uri);

				var str = new StringContent(sez,
				                            Encoding.Unicode,
				                            MediaTypeConstants.ApplicationJson);

				try
				{
					var response = await client.PostAsync(apiEndpoint,
					                                      str,
					                                      cancellationToken);
					return response;
				}

				catch (Exception exp)
				{
					LogHelper.LogErrorWithLineNumber(Logger,
					                                 exp.Message,
					                                 exp);
				}
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");

			return null;
		}

		private static void DefineDefaultRequestHeaders(this HttpClient client)
		{
			// Validate.
			client.ThrowArgumentExceptionWhenNull();

			// Create request headers.
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeConstants.ApplicationJson));
		}
	}
}