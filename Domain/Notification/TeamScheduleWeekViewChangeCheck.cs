using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface ITeamScheduleWeekViewChangeCheck
	{
		void InitiateNotify(ScheduleChangeForWeekViewEvent model);
	}

	public interface ITeamScheduleWeekViewChangedInDefaultScenario { }

	public class TeamScheduleWeekViewChangeCheck : ITeamScheduleWeekViewChangeCheck
	{
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly IMessageBrokerComposite _messageBroker;

		public TeamScheduleWeekViewChangeCheck(
			IScheduleDayReadModelRepository scheduleDayReadModelRepository,
			IMessageBrokerComposite messageBroker)
		{
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_messageBroker = messageBroker;
		}

		[EnabledBy(Toggles.WfmTeamSchedule_OnlyRefreshScheduleForRelevantChangesInWeekView_80242)]
		public void InitiateNotify(ScheduleChangeForWeekViewEvent @event)
		{
			if (IsRelevantChange(@event.Date, @event.Person, @event.NewReadModel))
			{
				_messageBroker.Send(
					@event.LogOnDatasource,
					@event.LogOnBusinessUnitId,
					@event.Date.Date,
					@event.Date.Date,
					Guid.Empty,
					@event.Person.Id.GetValueOrDefault(),
					typeof(Person),
					Guid.Empty,
					typeof(ITeamScheduleWeekViewChangedInDefaultScenario),
					DomainUpdateType.NotApplicable,
					null);
			}
		}

		private bool IsRelevantChange(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel)
		{
			var existingReadModel = _scheduleDayReadModelRepository.ForPerson(date, person.Id.GetValueOrDefault());

			return existingReadModel?.Workday != newReadModel?.Workday
				|| existingReadModel?.StartDateTime != newReadModel?.StartDateTime
				|| existingReadModel?.EndDateTime != newReadModel?.EndDateTime
				|| existingReadModel?.Label != newReadModel?.Label;
		}
	}

	public class ScheduleChangeForWeekViewEvent : EventWithLogOnContext
	{
		public DateOnly Date { get; set; }
		public IPerson Person { get; set; }
		public ScheduleDayReadModel NewReadModel { get; set; }
	}
}
