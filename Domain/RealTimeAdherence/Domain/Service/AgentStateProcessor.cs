﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NPOI.HSSF.Record.Chart;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tracer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class ProcessResult
	{
		public bool Processed;
		public AgentState State;
		public IEnumerable<IEvent> Events;
		public StateTraceLog TraceLog;

		// for logging
		public override string ToString() => $"Processed: {Processed}, State: {State}, Events: {Events.StringJoin(x => x.GetType().Name, ", ")}";
	}

	public class ProcessInput
	{
		public readonly DateTime CurrentTime;
		public readonly DeadLockVictim DeadLockVictim;
		public readonly ProperAlarm AppliedAlarm;
		public readonly IEnumerable<ScheduledActivity> Schedule;
		public readonly StateMapper StateMapper;
		public readonly InputInfo Input;
		public readonly AgentState Stored;
		public readonly StateTraceLog TraceLog;

		public ProcessInput(
			DateTime currentTime,
			DeadLockVictim deadLockVictim,
			InputInfo input,
			AgentState stored,
			IEnumerable<ScheduledActivity> schedule,
			StateMapper stateMapper,
			ProperAlarm appliedAlarm,
			StateTraceLog traceLog)
		{
			CurrentTime = currentTime;
			DeadLockVictim = deadLockVictim;
			Input = input;
			Stored = stored;
			Schedule = schedule;
			StateMapper = stateMapper;
			AppliedAlarm = appliedAlarm;
			TraceLog = traceLog;
		}

		// for logging
		public override string ToString() => $"CurrentTime: {CurrentTime}, DeadLockVictim: {DeadLockVictim}, Schedule: {Schedule.StringJoin(x => x.Name, ", ")}, Input: {Input}";
	}

	public class AgentStateProcessor
	{
		private readonly ShiftEventPublisher _shiftEventPublisher;
		private readonly ActivityEventPublisher _activityEventPublisher;
		private readonly StateEventPublisher _stateEventPublisher;
		private readonly RuleEventPublisher _ruleEventPublisher;
		private readonly AdherenceEventPublisher _adherenceEventPublisher;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly ICurrentEventPublisher _currentEventPublisher;
		private readonly ILateForWorkEventPublisher _lateForWorkEventPublisher;
		private readonly IRtaTracer _tracer;

		private const int threshold = 59;

		public AgentStateProcessor(
			ShiftEventPublisher shiftEventPublisher,
			ActivityEventPublisher activityEventPublisher,
			StateEventPublisher stateEventPublisher,
			RuleEventPublisher ruleEventPublisher,
			AdherenceEventPublisher adherenceEventPublisher,
			IEventPublisherScope eventPublisherScope,
			ICurrentEventPublisher currentEventPublisher,
			ILateForWorkEventPublisher lateForWorkEventPublisher,
			IRtaTracer tracer
		)
		{
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_ruleEventPublisher = ruleEventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
			_eventPublisherScope = eventPublisherScope;
			_currentEventPublisher = currentEventPublisher;
			_lateForWorkEventPublisher = lateForWorkEventPublisher;
			_tracer = tracer;
		}

		[LogInfo]
		public virtual ProcessResult Process(ProcessInput input)
		{
			var eventCollector = new EventCollector();
			using (_eventPublisherScope.OnThisThreadPublishTo(eventCollector))
			{
				var resultState = processRelevantMoments(input);
				var processed = resultState != null;

				if (!processed)
					_tracer.NoChange(input.TraceLog);

				return new ProcessResult
				{
					Processed = processed,
					Events = eventCollector.Pop(),
					TraceLog = input.TraceLog,
					State = resultState
				};
			}
		}

		private AgentState processRelevantMoments(ProcessInput processInput)
		{
			var times = new[] {processInput.CurrentTime};

			if (processInput.Stored.ReceivedTime != null)
			{
				var from = processInput.Stored.ReceivedTime;
				var to = processInput.CurrentTime;

				var startingActivities = processInput.Schedule
					.Where(x => x.StartDateTime > from && x.StartDateTime <= to)
					.Select(x => x.StartDateTime);

				var endingActivities = processInput.Schedule
					.Where(x => x.EndDateTime > from && x.EndDateTime <= to)
					.Select(x => x.EndDateTime);

				times = times
					.Concat(startingActivities)
					.Concat(endingActivities)
					.Distinct()
					.OrderBy(x => x)
					.ToArray();
			}

			var workingState = processInput.Stored;
			AgentState outState = null;
			times.ForEach(time =>
			{
				var input = time == processInput.CurrentTime ? processInput.Input : null;
				var context = new Context(
					time,
					processInput.DeadLockVictim,
					input,
					workingState,
					processInput.Schedule,
					processInput.StateMapper,
					processInput.AppliedAlarm);
				if (context.ShouldProcessState())
				{
					new ChocaChocaDoer().DoesItNow(context);
					process(context);
					workingState = context.MakeAgentState();
					outState = workingState;
				}
			});

			return outState;
		}

		private class ChocaChocaDoer
		{
			public void DoesItNow(Context context)
			{
				context.ArrivingAfterLateForWork = context.Stored.LateForWork &&
												   context.State.IsLoggedIn() &&
												   isOutsideTreshold(context.Time, context.Schedule.CurrentShiftStartTime);

				var lateForWork = context.Schedule.ShiftStarted() && context.State.IsLoggedOut();
				if (!context.ArrivingAfterLateForWork)
					context.LateForWork = context.Stored.LateForWork || lateForWork;

				if (context.Schedule.ShiftEnded())
				{
					context.LateForWork = false;
					context.ArrivingAfterLateForWork = false;
				}
			}
		}

		private static bool isOutsideTreshold(DateTime stateTime, DateTime shiftStart)
		{
			var timeSinceShiftStart = stateTime - shiftStart;
			return timeSinceShiftStart.TotalSeconds > threshold;
		}


		private void process(Context context)
		{
			_shiftEventPublisher.Publish(context);
			_activityEventPublisher.Publish(context);
			_stateEventPublisher.Publish(context);
			_ruleEventPublisher.Publish(context);
			_adherenceEventPublisher.Publish(context);
			_lateForWorkEventPublisher.Publish(context);

			_currentEventPublisher.Current().Publish(new AgentStateChangedEvent
			{
				PersonId = context.PersonId,
				Time = context.Time,
				CurrentActivityName = context.Schedule.CurrentActivityName(),
				NextActivityName = context.Schedule.NextActivityName(),
				NextActivityStartTime = context.Schedule.NextActivityStartTime(),
				ActivitiesInTimeWindow = context.Schedule.ActivitiesInTimeWindow()
			});
		}
	}
}