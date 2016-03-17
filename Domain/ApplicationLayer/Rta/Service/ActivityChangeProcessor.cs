using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ActivityChangeProcessor
	{
		private readonly INow _now;
		private readonly IContextLoader _contextLoader;
		private readonly RtaProcessor _processor;

		public ActivityChangeProcessor(
			INow now,
			IContextLoader contextLoader,
			RtaProcessor processor
			)
		{
			_now = now;
			_contextLoader = contextLoader;
			_processor = processor;
		}

		// tenant in key? customer installs have unique person ids...?
		private readonly Dictionary<Guid, currentPeriod> _currentPeriodInfo = new Dictionary<Guid, currentPeriod>();

		public void CheckForActivityChanges()
		{
			_contextLoader.ForAll(person =>
			{
				var current = new currentPeriod();
				var currentActivity = person.Schedule.CurrentActivity();
				if (currentActivity != null)
				{
					current.StartTime = currentActivity.StartDateTime;
					current.EndTime = currentActivity.EndDateTime;
					current.ActivityId = currentActivity.PayloadId;
				}
				else
				{
					var previousActivity = person.Schedule.PreviousActivity();
					if (previousActivity != null)
						current.StartTime = previousActivity.EndDateTime;
					var nextActivity = person.Schedule.NextActivity();
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
				_processor.Process(person);

				_currentPeriodInfo[person.PersonId] = current;
			});

		}

		private class currentPeriod
		{
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
			public Guid ActivityId { get; set; }
		}

	}
}