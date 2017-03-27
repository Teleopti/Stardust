using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	public class OriginHandlerPipelineModuleTest
	{
		[Test]
		public void ShouldRejectRequestsWithInconsistentOrigin()
		{
			var target = new OriginHandlerPipelineModule();
			var function = target.BuildAuthorizeConnect((d, r) => true);

			var request = new FakeHubRequest {Url = new Uri("http://burk1/TeleoptiWFM/Web/signalr?connect")};
			var headers = new SimpleSignalrNameValueCollection(new NameValueCollection())
			{
				{"Origin", "http://attacker-website/TeleoptiWFM"}
			};
			request.Headers = headers;
			function(new HubDescriptor(), request).Should().Be.False();
		}

		[Test]
		public void ShouldAcceptRequestsWithoutOrigin()
		{
			var target = new OriginHandlerPipelineModule();
			var function = target.BuildAuthorizeConnect((d, r) => true);

			var request = new FakeHubRequest {Url = new Uri("http://burk1/TeleoptiWFM/Web/signalr?connect")};
			var headers = new SimpleSignalrNameValueCollection(new NameValueCollection());
			request.Headers = headers;
			function(new HubDescriptor(), request).Should().Be.True();
		}

		[Test]
		public void ShouldAcceptRequestsWithIpAddressOriginFromProxy()
		{
			var target = new OriginHandlerPipelineModule();
			var function = target.BuildAuthorizeConnect((d, r) => true);

			var request = new FakeHubRequest { Url = new Uri("http://burk1/TeleoptiWFM/Web/signalr?connect") };
			var headers = new SimpleSignalrNameValueCollection(new NameValueCollection
			{
				{"Origin", "http://10.0.0.1/TeleoptiWFM"}
			});
			request.Headers = headers;
			function(new HubDescriptor(), request).Should().Be.True();
		}

		[Test]
		public void ShouldAcceptRequestsWithIpAddressHostFromProxy()
		{
			var target = new OriginHandlerPipelineModule();
			var function = target.BuildAuthorizeConnect((d, r) => true);

			var request = new FakeHubRequest { Url = new Uri("http://10.0.0.1/TeleoptiWFM/Web/signalr?connect") };
			var headers = new SimpleSignalrNameValueCollection(new NameValueCollection
			{
				{"Origin", "http://burk/TeleoptiWFM/Web/signalr?connect"}
			});
			request.Headers = headers;
			function(new HubDescriptor(), request).Should().Be.True();
		}
	}

	public class SimpleSignalrNameValueCollection : INameValueCollection
	{
		private readonly NameValueCollection _underlyingCollection;

		public SimpleSignalrNameValueCollection(NameValueCollection underlyingCollection)
		{
			_underlyingCollection = underlyingCollection;
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return GetEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable<KeyValuePair<string, string>> GetEnumerable()
		{
			foreach (var key in _underlyingCollection.AllKeys)
				yield return new KeyValuePair<string, string>(key, _underlyingCollection.Get(key));
		}

		public IEnumerable<string> GetValues(string key)
		{
			return _underlyingCollection.GetValues(key);
		}

		public string Get(string key)
		{
			return _underlyingCollection.Get(key);
		}

		public string this[string key] => _underlyingCollection[key];

		public void Add(string key, string value)
		{
			_underlyingCollection.Add(key,value);
		}
	}

	public class FakeHubRequest : IRequest
	{
		public Task<INameValueCollection> ReadForm()
		{
			throw new NotImplementedException();
		}

		public Uri Url { get; set; }
		public string LocalPath { get; }
		public INameValueCollection QueryString { get; }
		public INameValueCollection Headers { get; set; }
		public IDictionary<string, Cookie> Cookies { get; }
		public IPrincipal User { get; }
		public IDictionary<string, object> Environment { get; }
	}
}