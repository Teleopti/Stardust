using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Stardust.Node.Interfaces
{
	public interface IHttpSender
	{
		Task<HttpResponseMessage> PostAsync(Uri url, object data, CancellationToken cancellationToken);
	}
}