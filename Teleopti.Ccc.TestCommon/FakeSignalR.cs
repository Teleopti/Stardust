using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeSignalR : ISignalR
	{
		public string AddedConnection;
		public string AddedConnectionToGroup;

		public void AddConnectionToGroup(string groupName, string connectionId)
		{
			AddedConnection = connectionId;
			AddedConnectionToGroup = groupName;
		}




		public string RemovedConnection;
		public string RemovedConnectionFromGroup;

		public void RemoveConnectionFromGroup(string groupName, string connectionId)
		{
			RemovedConnection = connectionId;
			RemovedConnectionFromGroup = groupName;
		}




		public Message SentMessage;
		public string SentToGroup;
		public string SentRoute;

		public ConcurrentBag<string> SentToGroups = new ConcurrentBag<string>();
		public ConcurrentBag<string> SentRoutes = new ConcurrentBag<string>();
		public ConcurrentBag<Message> SentMessages = new ConcurrentBag<Message>();

		public void CallOnEventMessage(string groupName, string route, Message message)
		{
			SentToGroup = groupName;
			SentRoute = route;
			SentMessage = message;

			SentToGroups.Add(groupName);
			SentRoutes.Add(route);
			SentMessages.Add(message);
		}
	}
}