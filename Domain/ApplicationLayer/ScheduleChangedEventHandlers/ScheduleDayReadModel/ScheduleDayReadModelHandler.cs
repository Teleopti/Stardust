using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
	public class ScheduleDayReadModelHandler : IHandleEvent<ProjectionChangedEvent>, IHandleEvent<ProjectionChangedEventForScheduleDay>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IDoNotifySmsLink _notifySmsLink;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sms")]
		public ScheduleDayReadModelHandler(IUnitOfWorkFactory unitOfWorkFactory,
		                                   IPersonRepository personRepository,
		                                   IDoNotifySmsLink notifySmsLink,
		                                   IScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
		                                   IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personRepository = personRepository;
			_notifySmsLink = notifySmsLink;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
		}

		private void createReadModel(ProjectionChangedEventBase message)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (!message.IsDefaultScenario) return;

				var person = _personRepository.Get(message.PersonId);

				foreach (var denormalizedScheduleDay in message.ScheduleDays)
				{
					var date = new DateOnly(denormalizedScheduleDay.Date);
					var dateOnlyPeriod = new DateOnlyPeriod(date, date);

					var readModel = _scheduleDayReadModelsCreator.GetReadModel(denormalizedScheduleDay, person);

					if (!message.IsInitialLoad)
					{
						_notifySmsLink.NotifySmsLink(readModel, date, person);
					}

					if (!message.IsInitialLoad)
					{
						_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
					}
					_scheduleDayReadModelRepository.SaveReadModel(readModel);

					uow.PersistAll();
				}
			}
		}

		public void Handle(ProjectionChangedEventForScheduleDay @event)
		{
			createReadModel(@event);
		}
	}
}