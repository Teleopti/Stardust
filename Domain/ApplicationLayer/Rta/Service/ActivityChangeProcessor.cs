using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeProcessor
	{
		private readonly INow _now;
		private readonly IPersonLoader _personLoader;
		private readonly IDatabaseLoader _databaseLoader;
		private readonly RtaProcessor _processor;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IAgentStateMessageSender _agentStateMessageSender;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IPreviousStateInfoLoader _previousStateInfoLoader;

		public ActivityChangeProcessor(
			INow now,
			IPersonLoader personLoader,
			IDatabaseLoader databaseLoader, 
			RtaProcessor processor,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IAgentStateMessageSender agentStateMessageSender,
			IAdherenceAggregator adherenceAggregator,
			IPreviousStateInfoLoader previousStateInfoLoader
			)
		{
			_now = now;
			_personLoader = personLoader;
			_databaseLoader = databaseLoader;
			_processor = processor;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_agentStateMessageSender = agentStateMessageSender;
			_adherenceAggregator = adherenceAggregator;
			_previousStateInfoLoader = previousStateInfoLoader;
		}

		// tenant in key? customer installs have unique person ids...?
		private readonly Dictionary<Guid, currentPeriod> _currentPeriodInfo = new Dictionary<Guid, currentPeriod>();

		public void CheckForActivityChanges()
		{
			var now = _now.UtcDateTime();

			var persons = _personLoader.LoadAllPersonsData();

			persons.ForEach(person =>
			{
				var schedule = _databaseLoader.GetCurrentSchedule(person.PersonId);

				var current = new currentPeriod();
				var currentActivity = ScheduleInfo.ActivityForTime(schedule, now);
				if (currentActivity != null)
				{
					current.StartTime = currentActivity.StartDateTime;
					current.EndTime = currentActivity.EndDateTime;
					current.ActivityId = currentActivity.PayloadId;
				}
				else
				{
					var previousActivity = ScheduleInfo.PreviousActivity(schedule, now);
					if (previousActivity != null)
						current.StartTime = previousActivity.EndDateTime;
					var nextActivity = ScheduleInfo.NextActivity(schedule, now);
					if (nextActivity != null)
						current.EndTime = nextActivity.StartDateTime;
				}

				currentPeriod previous;
				_currentPeriodInfo.TryGetValue(person.PersonId, out previous);

				var doProcess =
					previous == null ||
					previous.StartTime != current.StartTime ||
					previous.EndTime != current.EndTime ||
					previous.ActivityId != current.ActivityId;

				if (!doProcess) return;

				process(null, person);
				_currentPeriodInfo[person.PersonId] = current;
			});

		}

		private class currentPeriod
		{
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
			public Guid ActivityId { get; set; }
		}

		private void process(ExternalUserStateInputModel input, PersonOrganizationData person)
		{
			_processor.Process(
				new RtaProcessContext(
					input,
					person,
					_now,
					_agentStateReadModelUpdater,
					_agentStateMessageSender,
					_adherenceAggregator,
					_previousStateInfoLoader
					));
		}

	}
}