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

		public void Post(HttpClient client, string uri, HttpContent httpContent)
		{
			Requests.Add(new RequestInfo {Client = client, Uri = uri, HttpContent = httpContent});
		}

		public void PostOrThrow(HttpClient client, string uri, HttpContent httpContent)
		{
			Requests.Add(new RequestInfo { Client = client, Uri = uri, HttpContent = httpContent });
			if (_exception != null)
				throw _exception;
		}

		public string GetOrThrow(HttpClient client, string uri)
		{
			Requests.Add(new RequestInfo {Client = client, Uri = uri});
			if (_exception != null)
				throw _exception;
			var result = JsonConvert.SerializeObject(_messages);
			_messages.Clear();
			return result;
		}

		public void Down()
		{
			Throws(new AggregateException(new HttpRequestException()));
		}

		public void GivesError(HttpStatusCode httpCode)
		{
			Throws(new HttpRequestException());
		}

		public void Succeeds()
		{
			Throws(null);
		}

		public void Throws(Exception type)
		{
			_exception = type;
		}

	}
}