using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public class RtaTracerLog<T>
	{
		public DateTime Time;
		public string Process;
		public string Message;
		public T Log;
	}

	public class ProcessReceivedLog
	{
		public DateTime? RecievedAt;
	}

	public class ProcessProcessingLog
	{
		public DateTime? ProcessingAt;
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