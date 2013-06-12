using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ScheduleMessageSender : IMessageSender
	{
		private readonly ISendDenormalizeNotification _sendDenormalizeNotification;
		private readonly ISaveToDenormalizationQueue _saveToDenormalizationQueue;
		private static readonly Type[] ExcludedTypes = new[] { typeof(INote), typeof(IPublicNote) };

		public ScheduleMessageSender(ISendDenormalizeNotification sendDenormalizeNotification, ISaveToDenormalizationQueue saveToDenormalizationQueue)
		{
			_sendDenormalizeNotification = sendDenormalizeNotification;
			_saveToDenormalizationQueue = saveToDenormalizationQueue;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = extractScheduleChangesOnly(modifiedRoots);
			if (!scheduleData.Any()) return;

			var people = scheduleData.Select(s => s.Person).Distinct();
			var scenarios = scheduleData.Select(s => s.Scenario).Distinct();
			var atLeastOneMessage = false;

			foreach (var person in people)
			{
				foreach (var scenario in scenarios)
				{
					if (scenario==null) continue;

					var matchedItems = scheduleData.Where(s => s.Scenario!=null && s.Scenario.Equals(scenario) && s.Person.Equals(person));
					if (!matchedItems.Any()) continue;

					var startDateTime = matchedItems.Min(s => s.Period.StartDateTime);
					var endDateTime = matchedItems.Max(s => s.Period.EndDateTime);

					var message = new ScheduleChangedEvent
					              	{
					              		ScenarioId = scenario.Id.GetValueOrDefault(),
										StartDateTime = startDateTime.AddHours(-24), //Bug fix for #23647
					              		EndDateTime = endDateTime,
					              		PersonId = person.Id.GetValueOrDefault(),
					              	};
					_saveToDenormalizationQueue.Execute(message);
					atLeastOneMessage = true;
				}
			}

			if (atLeastOneMessage)
			{
				_sendDenormalizeNotification.Notify();
			}
		}

		private static IEnumerable<IPersistableScheduleData> extractScheduleChangesOnly(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = modifiedRoots.Select(r => r.Root).OfType<IPersistableScheduleData>();
			scheduleData = scheduleData.Where(s => !ExcludedTypes.Any(t => s.GetType().GetInterfaces().Contains(t)));
			return scheduleData;
		}
	}
}