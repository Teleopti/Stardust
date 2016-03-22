using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Node.ActionResults
{
	public class BadRequestWithReasonPhrase : IHttpActionResult
	{
		public string ReasonPhrase { get; private set; }

		public BadRequestWithReasonPhrase(string reasonPhrase)
		{
			ReasonPhrase = reasonPhrase;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return new Task<HttpResponseMessage>(() => new HttpResponseMessage(HttpStatusCode.BadRequest)
			{
				ReasonPhrase = ReasonPhrase
			});
		}
	}
}