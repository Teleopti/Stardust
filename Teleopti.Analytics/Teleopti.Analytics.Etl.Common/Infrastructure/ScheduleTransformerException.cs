using System;
using System.Runtime.Serialization;
using System.Security;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	[Serializable]
	public class ScheduleTransformerException : Exception
	{
		private readonly string _message;
		private readonly string _stackTrace;

		public ScheduleTransformerException() { }

		public ScheduleTransformerException(string message, string stackTrace)
			: this()
		{
			_message = message;
			_stackTrace = stackTrace;
		}

		protected ScheduleTransformerException(SerializationInfo info, StreamingContext context):base(info, context)
		{
			_stackTrace = info.GetString("StackTrace");
			_message = info.GetString("Message");
		}

		public override string StackTrace
		{
			get
			{
				return _stackTrace;
			}
		}

		public override string Message
		{
			get
			{
				return _message;
			}
		}

		[SecurityCritical]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("StackTrace", _stackTrace);
			info.AddValue("Message", _message);
			base.GetObjectData(info, context);
		}
	}
}