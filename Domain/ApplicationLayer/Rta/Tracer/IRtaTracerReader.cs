using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public interface IRtaTracerReader
	{
		IEnumerable<RtaTracerLog<T>> ReadOfType<T>();
	}

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