using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http
{
	public class FakeHttpServer : IHttpServer
	{
		private readonly IList<Message> _messages = new List<Message>();
		private bool _shouldFail;
		private HttpStatusCode _statusCode;
		private Exception _exception;

		public class RequestInfo
		{
			public HttpClient Client;
			public string Uri;
			public HttpContent HttpContent;
		}

		public readonly IList<RequestInfo> Requests = new List<RequestInfo>();
		
		public void Has(Message message)
		{
			_messages.Add(message);
		}

		public HttpResponseMessage PostAsync(HttpClient client, string uri, HttpContent httpContent)
		{
			Requests.Add(new RequestInfo {Client = client, Uri = uri, HttpContent = httpContent});

			if (_exception != null)
				throw _exception;
			if (_shouldFail)
				return new HttpResponseMessage {StatusCode = _statusCode};

			return new HttpResponseMessage();
		}

		public HttpResponseMessage Get(HttpClient client, string uri)
		{
			Requests.Add(new RequestInfo {Client = client, Uri = uri});

			if (_exception != null)
				throw _exception;
			if (_shouldFail)
				return new HttpResponseMessage { StatusCode = _statusCode };

			var result = JsonConvert.SerializeObject(_messages);
			_messages.Clear();

			return new HttpResponseMessage {Content = new StringContent(result)};
		}

		public void Fails(HttpStatusCode statusCode)
		{
			_shouldFail = true;
			_statusCode = statusCode;
		}

		public void Succeeds()
		{
			_shouldFail = false;
			_statusCode = HttpStatusCode.OK;
		}

		public void Throws(Exception type)
		{
			_exception = type;
		}
	}
}