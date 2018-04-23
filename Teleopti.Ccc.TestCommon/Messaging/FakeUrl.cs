﻿using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.TestCommon.Messaging
{
	public class FakeUrl : IMessageBrokerUrl
	{
		private string _url;
		
		public FakeUrl(string url)
		{
			_url = url;
		}

		public void Configure(string url)
		{
			_url = url;
		}

		public string Url { get { return _url; } }

		public void Is(string url)
		{
			_url = url;
		}
	}
}