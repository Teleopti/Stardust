using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.States
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
		public readonly ExternalLogonMapper ExternalLogonMapper;
		public readonly BelongsToDateMapper BelongsToDateMapper;
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
			ExternalLogonMapper externalLogonMapper,
			BelongsToDateMapper belongsToDateMapper,
			ProperAlarm appliedAlarm,
			StateTraceLog traceLog)
		{
			ExternalLogonMapper = externalLogonMapper;
			BelongsToDateMapper = belongsToDateMapper;
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
		private readonly LateForWorkEventPublisher _lateForWorkEventPublisher;
		private readonly IRtaTracer _tracer;
		private readonly IAdherenceDayStartEventPublisher _adherenceDayStartEventPublisher;

		public AgentStateProcessor(
			ShiftEventPublisher shiftEventPublisher,
			ActivityEventPublisher activityEventPublisher,
			StateEventPublisher stateEventPublisher,
			RuleEventPublisher ruleEventPublisher,
			AdherenceEventPublisher adherenceEventPublisher,
			IEventPublisherScope eventPublisherScope,
			ICurrentEventPublisher currentEventPublisher,
			LateForWorkEventPublisher lateForWorkEventPublisher,
			IRtaTracer tracer,
			IAdherenceDayStartEventPublisher adherenceDayStartEventPublisher)
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
			_adherenceDayStartEventPublisher = adherenceDayStartEventPublisher;
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
			var relevantMoments = ScheduleInfo.RelevantMoments(processInput.Schedule, processInput.Stored.ReceivedTime, processInput.CurrentTime, true);

			var workingState = processInput.Stored;
			AgentState outState = null;
			relevantMoments.ForEach(time =>
			{
				var input = time == processInput.CurrentTime ? processInput.Input : null;
				var context = new Context(
					time,
					processInput.DeadLockVictim,
					input,
					workingState,
					processInput.Schedule,
					processInput.StateMapper,
					processInput.ExternalLogonMapper,
					processInput.BelongsToDateMapper,
					processInput.AppliedAlarm
					);
				if (context.ShouldProcessState())
				{
					process(context);
					workingState = context.MakeAgentState();
					outState = workingState;
				}
			});

			return outState;
		}

		private void process(Context context)
		{
			_shiftEventPublisher.Publish(context);
			_activityEventPublisher.Publish(context);
			_stateEventPublisher.Publish(context);
			_ruleEventPublisher.Publish(context);
			_adherenceEventPublisher.Publish(context);
			_lateForWorkEventPublisher.Publish(context);
			_adherenceDayStartEventPublisher.Publish(context);

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