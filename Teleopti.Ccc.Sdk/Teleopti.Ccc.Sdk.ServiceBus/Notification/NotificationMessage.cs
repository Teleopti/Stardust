using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface INotificationMessage
	{
		string Subject { get; set; }
		IList<string> Messages { get;  }
	}

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