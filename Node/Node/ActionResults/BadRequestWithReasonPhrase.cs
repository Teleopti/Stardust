using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Node.ActionResults
{
	public class BadRequestWithReasonPhrase : IHttpActionResult
	{
		public BadRequestWithReasonPhrase(string reasonPhrase)
		{
			ReasonPhrase = reasonPhrase;
		}

		public string ReasonPhrase { get; private set; }

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.BadRequest)
			{
				ReasonPhrase = ReasonPhrase
			}, cancellationToken);
		}
	}
}