using System;
using System.Web;

namespace Teleopti.Ccc.Web.Core.Exceptions
{
	[Serializable]
	public class CustomMessageException : HttpException
	{
		public string Shortmessage { get; set; }

		public CustomMessageException(int httpCode, string message, string shortmessage)
			: base(httpCode, message)
		{
			Shortmessage = shortmessage;
		}

		public CustomMessageException(int httpCode, string message)
			: base(httpCode, message)
		{
			Shortmessage = message;
		}
	}
}