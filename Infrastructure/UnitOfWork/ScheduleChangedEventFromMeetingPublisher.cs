using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Meetings;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ScheduleChangedEventFromMeetingPublisher : IPersistCallback
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public ScheduleChangedEventFromMeetingPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var meetings =
				modifiedRoots.Select(r => new { ProvideCustomChangeInfo = r.Root as IProvideCustomChangeInfo, UpdateType = r.Status });
			meetings = meetings.Where(m => m.ProvideCustomChangeInfo != null);
			if (!meetings.Any()) return;

			var changedMeetings = meetings.Select(c =>
			{
				var tracker = new MeetingChangeTracker();
				tracker.TakeSnapshot((IMeeting)c.ProvideCustomChangeInfo.BeforeChanges());
				return new
				{

					Meeting = c,
					CustomChanges =
						tracker.CustomChanges((IMeeting)c.ProvideCustomChangeInfo,
											  c.UpdateType)
				};
			});

			foreach (var changedMeeting in changedMeetings)
			{
				foreach (var changedRoot in changedMeeting.CustomChanges)
				{
					var customChange = changedRoot.Root as ICustomChangedEntity;
					if (customChange == null) return;

					var message = new ScheduleChangedEvent
					              	{
					              		ScenarioId =
					              			((IMeeting) changedMeeting.Meeting.ProvideCustomChangeInfo).
					              			Scenario.Id.GetValueOrDefault(),
					              		StartDateTime = customChange.Period.StartDateTime,
					              		EndDateTime = customChange.Period.EndDateTime,
					              		PersonId = customChange.MainRoot.Id.GetValueOrDefault()
					              	};
					_eventPublisher.Publish(message);
				}
			}
		}
	}
}
