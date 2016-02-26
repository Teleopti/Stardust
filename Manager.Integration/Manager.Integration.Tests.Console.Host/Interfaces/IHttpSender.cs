using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Manager.IntegrationTest.Console.Host.Interfaces
{
	public interface IHttpSender
	{
		Task<HttpResponseMessage> PostAsync(Uri url,
											object data,
											CancellationToken cancellationToken);


		Task<HttpResponseMessage> PostAsync(Uri url,
											object data);

		Task<HttpResponseMessage> DeleteAsync(Uri url);

		Task<HttpResponseMessage> DeleteAsync(Uri url,
											  CancellationToken cancellationToken);

		Task<HttpResponseMessage> GetAsync(Uri url);

		Task<HttpResponseMessage> GetAsync(Uri url,
										   CancellationToken cancellationToken);

		Task<bool> TryGetAsync(Uri url);

		Task<bool> TryGetAsync(Uri url,
								CancellationToken cancellationToken);
	}
}