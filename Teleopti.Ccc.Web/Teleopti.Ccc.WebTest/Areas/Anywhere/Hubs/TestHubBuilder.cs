using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	public class TestHubBuilder
	{
		public dynamic FakeClient(string methodName, Action action)
		{
			IDictionary<string, object> caller = new ExpandoObject();
			caller[methodName] = action;
			return caller;
		}

		public dynamic FakeClient<T>(string methodName, Action<T> action)
		{
			IDictionary<string, object> caller = new ExpandoObject();
			caller[methodName] = action;
			return caller;
		}

		public dynamic FakeClient<T, T2>(string methodName, Action<T, T2> action)
		{
			IDictionary<string, object> caller = new ExpandoObject();
			caller[methodName] = action;
			return caller;
		}

		public void SetupHub(TestableHub hub)
		{
			hub.Context = new HubCallerContext(null, "connection");
			hub.Groups = MockRepository.GenerateMock<IGroupManager>();
			hub.Groups.Stub(x => x.Add(null, null)).IgnoreArguments().Return(DoneTask());
			hub.Groups.Stub(x => x.Remove(null, null)).IgnoreArguments().Return(DoneTask());
			hub.Clients = MockRepository.GenerateMock<IHubClientContext>();
		}

		public void SetupHub(TestableHub hub, dynamic client)
		{
			SetupHub(hub);
			hub.Clients.Stub(x => x.Caller).Return(client);
			hub.Clients.Stub(x => x.Group(null)).IgnoreArguments().Return(client);
		}

		private Task DoneTask()
		{
			var task = Task.Factory.StartNew(() => { });
			Task.WaitAll(task);
			return task;
		}
	}
}