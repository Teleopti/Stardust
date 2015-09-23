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
		private readonly IDatabaseLoader _databaseLoader;
		private readonly RtaProcessor _processor;
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IAgentStateMessageSender _agentStateMessageSender;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IPreviousStateInfoLoader _previousStateInfoLoader;

		public ActivityChangeProcessor(
			INow now,
			IDatabaseLoader databaseLoader,
			RtaProcessor processor,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IAgentStateMessageSender agentStateMessageSender,
			IAdherenceAggregator adherenceAggregator,
			IPreviousStateInfoLoader previousStateInfoLoader
			)
		{
			_now = now;
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

			var persons = from l in _databaseLoader.ExternalLogOns().Values
				from p in l
				select p;

			persons.ForEach(p =>
			{
				PersonOrganizationData person;
				if (!_databaseLoader.PersonOrganizationData().TryGetValue(p.PersonId, out person))
					return;
				person.BusinessUnitId = p.BusinessUnitId;
				
				var schedule = _databaseLoader.GetCurrentSchedule(person.PersonId);

				currentPeriod previous;
				_currentPeriodInfo.TryGetValue(person.PersonId, out previous);
				if (previous == null)
					previous = new currentPeriod();

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

				if (previous.StartTime == current.StartTime &&
					previous.EndTime == current.EndTime &&
					previous.ActivityId == current.ActivityId)
					return;

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
					_databaseLoader,
					_agentStateReadModelUpdater,
					_agentStateMessageSender,
					_adherenceAggregator,
					_previousStateInfoLoader
					));
		}

	}
}