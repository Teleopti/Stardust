using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Stardust.Node.ActionResults
{
	public class ConflictResultWithReasonPhrase : IHttpActionResult
	{
		public ConflictResultWithReasonPhrase(string reasonPhrase)
		{
			ReasonPhrase = reasonPhrase;
		}

		public string ReasonPhrase { get; private set; }

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return new Task<HttpResponseMessage>(() => new HttpResponseMessage(HttpStatusCode.Conflict)
			{
				ReasonPhrase = ReasonPhrase
			});
		}
	}
}