﻿using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Teleopti.Ccc.Web.Filters
{
	public class AuthenticationFailureResult : IHttpActionResult
	{
		public AuthenticationFailureResult(string reasonPhrase, HttpRequestMessage request)
		{
			ReasonPhrase = reasonPhrase;
			Request = request;
		}

		public string ReasonPhrase { get; private set; }

		public HttpRequestMessage Request { get; private set; }

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(execute());
		}

		private HttpResponseMessage execute()
		{
			var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
			{
				RequestMessage = Request,
				ReasonPhrase = ReasonPhrase
			};
			return response;
		}
	}
}