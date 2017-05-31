using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationMessage : INotificationMessage
	{
		public string Subject { get; set; } = "";
		public IList<string> Messages { get; } = new List<string>();
		public string CustomerName { get; set; } = "";
		public object Data { get; set; }
	}
}