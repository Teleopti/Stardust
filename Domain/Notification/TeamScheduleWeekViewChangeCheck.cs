using System;
using System.Linq;
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
			var dates = @event.NewReadModels
				.Where(readModel => isRelevantChange(readModel.Key, @event.Person, readModel.Value))
				.Select(readModel => readModel.Key.Date);

			if (dates.Any())
			{
				var changedDates = dates.ToList();
				changedDates.Sort();

				_messageBroker.Send(
						@event.LogOnDatasource,
						@event.LogOnBusinessUnitId,
						changedDates.First(),
						changedDates.Last(),
						Guid.Empty,
						@event.Person.Id.GetValueOrDefault(),
						typeof(Person),
						Guid.Empty,
						typeof(ITeamScheduleWeekViewChangedInDefaultScenario),
						DomainUpdateType.NotApplicable,
						null);
			}
		}

		private bool isRelevantChange(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel)
		{
			var existingReadModel = _scheduleDayReadModelRepository.ForPerson(date, person.Id.GetValueOrDefault());

			return existingReadModel?.Workday != newReadModel?.Workday
				|| existingReadModel?.StartDateTime != newReadModel?.StartDateTime
				|| existingReadModel?.EndDateTime != newReadModel?.EndDateTime
				|| existingReadModel?.Label != newReadModel?.Label;
		}
	}
}
