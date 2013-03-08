using System;
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Rhino.Mocks;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	public class TestHubBuilder
	{
		public dynamic FakeCaller<T>(string methodName, Action<T> action)
		{
			IDictionary<string, object> caller = new ExpandoObject();
			caller[methodName] = action;
			return caller;
		}

		public void SetupHub(Hub hub, dynamic caller)
		{
			hub.Context = new HubCallerContext(null, "connection");
			hub.Groups = MockRepository.GenerateMock<IGroupManager>();
			hub.Clients.Caller = caller;
		}
	}
}