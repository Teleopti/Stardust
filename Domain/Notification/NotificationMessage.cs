using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationMessage : INotificationMessage
	{
		private string _subject = "";
		private readonly IList<string> _messages = new List<string>();

		public string Subject
		{
			get { return _subject; }
			set { _subject = value; }
		}

		public IList<string> Messages
		{
			get { return _messages; }
		}
	}
}