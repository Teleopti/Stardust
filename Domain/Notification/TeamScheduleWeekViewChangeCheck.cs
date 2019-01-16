using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Domain.Notification
{
	public interface ITeamScheduleWeekViewChangeCheck
	{
		bool IsRelevantChange(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel);
		void InitiateNotify(TeamScheduleWeekViewChangeCheckModel model);
	}

	public interface ITeamScheduleWeekViewChange { }

	public class TeamScheduleWeekViewChangeCheck : ITeamScheduleWeekViewChangeCheck
	{
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly IMessageBrokerComposite _messageBroker;

		public TeamScheduleWeekViewChangeCheck(
			IScheduleDayReadModelRepository scheduleDayReadModelRepository,
			ICurrentDataSource currentDataSource,
			IMessageBrokerComposite messageBroker)
		{
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_messageBroker = messageBroker;
		}

		public void InitiateNotify(TeamScheduleWeekViewChangeCheckModel model)
		{
			if (IsRelevantChange(model.Date, model.Person, model.NewReadModel))
			{
				_messageBroker.Send(
					model.LogOnDatasource,
					model.LogOnBusinessUnitId,
					model.Date.Date,
					model.Date.Date,
					Guid.Empty,
					model.Person.Id.GetValueOrDefault(),
					typeof(Person),
					Guid.Empty,
					typeof(ITeamScheduleWeekViewChange),
					DomainUpdateType.NotApplicable,
					null);
			}
		}

		public bool IsRelevantChange(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel)
		{
			var existingReadModel = _scheduleDayReadModelRepository.ForPerson(date, person.Id.GetValueOrDefault());

			if (existingReadModel?.Workday != newReadModel?.Workday
				|| existingReadModel?.StartDateTime != newReadModel?.StartDateTime
				|| existingReadModel?.EndDateTime != newReadModel?.EndDateTime
				|| existingReadModel?.Label != newReadModel?.Label)
			{
				return true;
			}

			return false;
		}
	}

	public class TeamScheduleWeekViewChangeCheckModel : EventWithLogOnContext
	{
		public DateOnly Date { get; set; }
		public IPerson Person { get; set; }
		public ScheduleDayReadModel NewReadModel { get; set; }
	}
}
