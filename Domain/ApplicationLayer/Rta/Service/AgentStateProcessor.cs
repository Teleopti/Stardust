using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ProcessResult
	{
		public bool Processed;
		public AgentState State;
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

		public ProcessInput(
			DateTime currentTime,
			DeadLockVictim deadLockVictim,
			InputInfo input,
			AgentState stored,
			IEnumerable<ScheduledActivity> schedule,
			StateMapper stateMapper,
			ProperAlarm appliedAlarm)
		{
			CurrentTime = currentTime;
			DeadLockVictim = deadLockVictim;
			Input = input;
			Stored = stored;
			Schedule = schedule;
			StateMapper = stateMapper;
			AppliedAlarm = appliedAlarm;
		}

	}

	public class AgentStateProcessor
	{
		private readonly ShiftEventPublisher _shiftEventPublisher;
		private readonly ActivityEventPublisher _activityEventPublisher;
		private readonly StateEventPublisher _stateEventPublisher;
		private readonly RuleEventPublisher _ruleEventPublisher;
		private readonly AdherenceEventPublisher _adherenceEventPublisher;
		private readonly ICurrentEventPublisher _eventPublisher;

		public AgentStateProcessor(
			ShiftEventPublisher shiftEventPublisher,
			ActivityEventPublisher activityEventPublisher,
			StateEventPublisher stateEventPublisher,
			RuleEventPublisher ruleEventPublisher,
			AdherenceEventPublisher adherenceEventPublisher,
			ICurrentEventPublisher eventPublisher)
		{
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_ruleEventPublisher = ruleEventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
			_eventPublisher = eventPublisher;
		}

		[LogInfo]
		public virtual ProcessResult Process(ProcessInput input)
		{
			var resultState = processRelevantMoments(input);

			return new ProcessResult
			{
				Processed = resultState != null,
				State = resultState
			};
		}

		private AgentState processRelevantMoments(ProcessInput processInput)
		{
			var times = new[] {processInput.CurrentTime };

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

			_eventPublisher.Current().Publish(new AgentStateChangedEvent
			{
				PersonId = context.PersonId,
				Time = context.Time,
				CurrentActivityName = context.Schedule.CurrentActivityName(),
				NextActivityName = context.Schedule.NextActivityName(),
				NextActivityStartTime = context.Schedule.NextActivityStartTime(),
				ActivitiesInTimeWindow = context.Schedule.ActivitiesInTimeWindow(),
			});
		}
	}
}
