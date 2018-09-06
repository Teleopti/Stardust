using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ScheduleChangedEventFromMeetingPublisher : ITransactionHook
	{
		private readonly IEventPopulatingPublisher _eventPublisher;

		public ScheduleChangedEventFromMeetingPublisher(IEventPopulatingPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
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

			var messages = changedMeetings
				.SelectMany(c =>
					c.CustomChanges.Select(cc => cc.Root).OfType<ICustomChangedEntity>().Select(cc => new {cc, m = c}))
				.Select(c => new ScheduleChangedEvent
				{
					ScenarioId =
						((IMeeting) c.m.Meeting.ProvideCustomChangeInfo).Scenario.Id.GetValueOrDefault(),
					StartDateTime = c.cc.Period.StartDateTime,
					EndDateTime = c.cc.Period.EndDateTime,
					PersonId = c.cc.MainRoot.Id.GetValueOrDefault()
				}).ToArray();
			_eventPublisher.Publish(messages);
		}
	}
}
