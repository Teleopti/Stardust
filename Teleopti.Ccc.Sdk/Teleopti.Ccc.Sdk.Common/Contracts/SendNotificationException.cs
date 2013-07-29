using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
	[Serializable]
	public class SendNotificationException : Exception
	{
		public SendNotificationException(string message) : base(message) { }

		public SendNotificationException(string message, Exception innerException)
			: base(message, innerException) { }

		protected SendNotificationException(SerializationInfo info,
									StreamingContext context)
			: base(info, context)
		{
		}
	}
}