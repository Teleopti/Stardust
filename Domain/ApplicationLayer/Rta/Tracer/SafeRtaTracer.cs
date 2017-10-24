using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public class SafeRtaTracer : IRtaTracer
	{
		private readonly RtaTracer _unsafe;
		private static readonly ILog Log = LogManager.GetLogger(typeof(SafeRtaTracer));

		public SafeRtaTracer(RtaTracer tracer)
		{
			_unsafe = tracer;
		}

		public void RefreshTracers() => _unsafe.RefreshTracers();
		public void FlushBuffer() => _unsafe.FlushBuffer();
		public void PurgeLogs() => _unsafe.PurgeLogs();

		public void Trace(string usercode) => _unsafe.Trace(usercode);
		public void Stop() => _unsafe.Stop();
		public void Clear() => _unsafe.Clear();

		public void ProcessReceived(string method, int? count) => safely(() => _unsafe.ProcessReceived(method, count));
		public void ProcessProcessing() => safely(() => _unsafe.ProcessProcessing());
		public void ProcessActivityCheck() => safely(() => _unsafe.ProcessActivityCheck());
		public void ProcessException(Exception exception) => safely(() => _unsafe.ProcessException(exception));
		public void For(IEnumerable<StateTraceLog> traces, Action<StateTraceLog> trace) => safely(() => _unsafe.For(traces, trace));
		public StateTraceLog StateReceived(string userCode, string stateCode) => safely(() => _unsafe.StateReceived(userCode, stateCode));
		public StateTraceLog ActivityCheck(Guid personId) => safely(() => _unsafe.ActivityCheck(personId));
		public StateTraceLog SnapshotLogout(Guid personId, string stateCode) => safely(() => _unsafe.SnapshotLogout(personId, stateCode));
		public void InvalidStateCode(StateTraceLog trace) => safely(() => _unsafe.InvalidStateCode(trace));
		public void StateProcessing(StateTraceLog trace) => safely(() => _unsafe.StateProcessing(trace));
		public void InvalidSourceId(StateTraceLog trace) => safely(() => _unsafe.InvalidSourceId(trace));
		public void InvalidUserCode(StateTraceLog trace) => safely(() => _unsafe.InvalidUserCode(trace));
		public void NoChange(StateTraceLog trace) => safely(() => _unsafe.NoChange(trace));
		public void StateProcessed(StateTraceLog trace, IEnumerable<IEvent> events) => safely(() => _unsafe.StateProcessed(trace, events));

		private T safely<T>(Func<T> func)
		{
			try
			{
				return func.Invoke();
			}
			catch (Exception e)
			{
				Log.Error("RtaTracer doesnt work", e);
				return default(T);
			}
		}

		private void safely(Action action)
		{
			try
			{
				action.Invoke();
			}
			catch (Exception e)
			{
				Log.Error("RtaTracer doesnt work", e);
			}
		}
	}
}