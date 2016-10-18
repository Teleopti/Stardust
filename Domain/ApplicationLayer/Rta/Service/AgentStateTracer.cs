using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateTracer
	{
		ITraceWriter Trace(Context context);
	}

	public class DisabledTracer : IAgentStateTracer
	{
		private readonly ITraceWriter _noTrace = new NoTrace();

		public ITraceWriter Trace(Context context)
		{
			return _noTrace;
		}
	}

	public interface ITraceWriter : IDisposable
	{
		void ProcessSkipped();
		void AgentStateUpdated();
		void EventsPublished(IEnumerable<IEvent> events);
		void Error();
	}

	public class NoTrace : ITraceWriter
	{
		public void Dispose()
		{
		}

		public void ProcessSkipped()
		{
		}

		public void AgentStateUpdated()
		{
		}

		public void EventsPublished(IEnumerable<IEvent> events)
		{
		}

		public void Error()
		{
		}
	}

	public class AgentStateTracer : IAgentStateTracer
	{
		private readonly Regex _regex;
		private readonly ITraceWriter _noTrace = new NoTrace();

		public AgentStateTracer(IConfigReader config)
		{
			_regex = new Regex(config.AppConfig("RtaAgentStateTraceMatch"));
		}

		public ITraceWriter Trace(Context context)
		{
			if (string.IsNullOrEmpty(context.UserCode))
				return _noTrace;
			return _regex.Match(context.UserCode).Success
				? new traceWriter(context)
				: _noTrace;
		}

		private class traceWriter : ITraceWriter
		{
			private static readonly ILog log = LogManager.GetLogger(typeof(AgentStateTracer));
			private readonly Context _context;
			private StringBuilder _message;

			public traceWriter(Context context)
			{
				_context = context;
			}

			private bool activityChecker()
			{
				// no state code on input assumes activity checker
				return string.IsNullOrEmpty(_context.Input.StateCode);
			}

			public void ProcessSkipped()
			{
				if (activityChecker()) return;
				append("SKIP ");
				appendInfo();
			}

			public void AgentStateUpdated()
			{
				append(activityChecker() ? "CHECK" : "STATE");
				appendInfo();
			}

			public void Error()
			{
				append("ERROR");
				appendInfo();
			}

			private void appendInfo()
			{
				append(_context.UserCode);
				append(_context.StateCode);
				append(_context.Schedule.CurrentActivityName());
			}

			public void EventsPublished(IEnumerable<IEvent> events)
			{
				events.ForEach(e =>
				{
					appendIfType<PersonShiftStartEvent>(e, x => $"SS({x.ShiftStartTime:HH:mm:ss})");
					appendIfType<PersonActivityStartEvent>(e, x => $"AS({x.StartTime:HH:mm:ss})");
					appendIfType<PersonShiftEndEvent>(e, x => $"SE({x.ShiftEndTime:HH:mm:ss})");
					appendIfType<PersonStateChangedEvent>(e, x => $"SC({x.Timestamp:HH:mm:ss})");
					appendIfType<PersonActivityStartEvent>(e, x => $"AS({x.StartTime::mm:ss})");
					appendIfType<PersonInAdherenceEvent>(e, x => $"IN({x.Timestamp:HH:mm:ss})");
					appendIfType<PersonOutOfAdherenceEvent>(e, x => $"OUT({x.Timestamp:HH:mm:ss})");
					appendIfType<PersonNeutralAdherenceEvent>(e, x => $"NEUT({x.Timestamp:HH:mm:ss})");
				});
			}

			private void append(string message)
			{
				if (_message == null)
				{
					_message = new StringBuilder();
					_message.Append(_context.CurrentTime.ToString("yyyy-MM-dd HH:mm:ss,fff"));
				}
				_message.Append(" ");
				_message.Append(message);
			}

			private void appendIfType<T>(object o, Func<T, string> message) where T : class
			{
				var typed = o as T;
				if (typed != null)
					append(message(typed));
			}

			public void Dispose()
			{
				if (_message != null)
					log.Debug(_message);
			}

		}
	}
}