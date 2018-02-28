using System;

namespace Teleopti.Ccc.Domain.Rta.Tracer
{
	public class RtaTracerLog<T>
	{
		public DateTime Time;
		public string Tenant;
		public string Process;
		public string Message;
		public T Log;
	}

	public class ProcessReceivedLog
	{
		public DateTime? ReceivedAt;
		public string ReceivedBy;
		public int ReceivedCount;
	}

	public class ProcessProcessingLog
	{
		public DateTime? ProcessingAt;
	}

	public class ProcessExceptionLog
	{
		public string Type;
		public string Info;
	}

	public class ActivityCheckLog
	{
		public DateTime? ActivityCheckAt;
	}

	public class TracingLog
	{
		public string Tracing;
	}

	public class StateTraceLog
	{
		public Guid Id;
		public string User;
		public string StateCode;
	}
}