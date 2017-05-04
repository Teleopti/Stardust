using System;

namespace Teleopti.Ccc.Domain.Notification
{
	[Serializable]
	public class FCMSendMessageResponse
	{
		public int success { get; set; }
		public int failure { get; set; }

		public FCMSendMessageResult[] results { get; set; }
		
	}

	[Serializable]
	public class FCMSendMessageResult
	{
		public string message_id { get; set; }
		public string registration_id { get; set; }

		public string error { get; set; }
	}
}