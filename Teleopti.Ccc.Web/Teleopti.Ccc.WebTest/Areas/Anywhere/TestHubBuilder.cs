using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	
	public class TestHubBuilder
	{
		public void SetupHub(Hub hub)
		{
			hub.Context = new HubCallerContext(null, "connection");
			hub.Groups = new FakeGroupManager();
			hub.Clients = new FakeClients<dynamic>();
		}

		public void SetupHub(Hub hub, dynamic client)
		{
			SetupHub(hub);
			hub.Clients = new FakeClients<dynamic>(client);
		}

		public dynamic FakeClient(string methodName, Action action)
		{
			return new FakeClientBuilder().Make(methodName, action);
		}

		public dynamic FakeClient<T>(string methodName, Action<T> action)
		{
			return new FakeClientBuilder().Make(methodName, action);
		}

		public dynamic FakeClient<T, T2>(string methodName, Action<T, T2> action)
		{
			return new FakeClientBuilder().Make(methodName, action);
		}
	}

	public class FakeGroupManager : IGroupManager
	{
		public Task Add(string connectionId, string groupName)
		{
			return Task.FromResult(false);
		}

		public Task Remove(string connectionId, string groupName)
		{
			return Task.FromResult(false);
		}
	}

	public class FakeClients<T> : IHubCallerConnectionContext<T>
	{
		private readonly dynamic _client;

		public FakeClients()
		{
		}

		public FakeClients(dynamic client)
		{
			_client = client;
		}

		public T AllExcept(params string[] excludeConnectionIds)
		{
			return _client;
		}

		public T Client(string connectionId)
		{
			return _client;
		}

		public T Clients(IList<string> connectionIds)
		{
			return _client;
		}

		public T Group(string groupName, params string[] excludeConnectionIds)
		{
			return _client;
		}

		public T Groups(IList<string> groupNames, params string[] excludeConnectionIds)
		{
			return _client;
		}

		public T User(string userId)
		{
			return _client;
		}

		public T Users(IList<string> userIds)
		{
			return _client;
		}

		public T All { get { return _client; } }

		public T OthersInGroup(string groupName)
		{
			return _client;
		}

		public T OthersInGroups(IList<string> groupNames)
		{
			return _client;
		}

		public T Caller { get { return _client; } }

		public dynamic CallerState { get; set; }

		public T Others { get { return _client; } }
	}

}