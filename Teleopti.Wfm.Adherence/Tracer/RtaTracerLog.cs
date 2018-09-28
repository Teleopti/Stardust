using System;

namespace Teleopti.Wfm.Adherence.Tracer
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
		public DateTime? At;
		public string By;
		public int Count;
	}

	public class ProcessEnqueuingLog
	{
		public DateTime? At;
		public int Count;
	}

	public class ProcessProcessingLog
	{
		public DateTime? At;
		public int Count;
	}

	public class ProcessActivityCheckLog
	{
		public DateTime? At;
	}

	public class ProcessExceptionLog
	{
		public string Type;
		public string Info;
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