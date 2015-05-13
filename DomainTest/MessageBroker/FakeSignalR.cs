using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.DomainTest.MessageBroker
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




		public Interfaces.MessageBroker.Notification SentMessage;
		public string SentToGroup;
		public string SentRoute;

		public IList<string> SentToGroups = new List<string>();
		public IList<string> SentRoutes = new List<string>();
		public IList<Interfaces.MessageBroker.Notification> SentMessages = new List<Interfaces.MessageBroker.Notification>();

		public void CallOnEventMessage(string groupName, string route, Interfaces.MessageBroker.Notification notification)
		{
			SentToGroup = groupName;
			SentRoute = route;
			SentMessage = notification;

			SentToGroups.Add(groupName);
			SentRoutes.Add(route);
			SentMessages.Add(notification);
		}
	}
}