using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ScheduleDayReadModelHandler : IHandleEvent<ProjectionChangedEvent>, IHandleEvent<ProjectionChangedEventForScheduleDay>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IDoNotifySmsLink _doNotifySmsLink;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sms")]
		public ScheduleDayReadModelHandler(IUnitOfWorkFactory unitOfWorkFactory,
		                           IPersonRepository personRepository,
			IDoNotifySmsLink doNotifySmsLink,
									IScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
									IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personRepository = personRepository;
			_doNotifySmsLink = doNotifySmsLink;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ProjectionChangedEvent message)
		{
			createReadModel(message);
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
						_doNotifySmsLink.NotifySmsLink(readModel, date, person);
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

		public void Handle(ProjectionChangedEventForScheduleDay message)
		{
			createReadModel(message);
		}
	}
}