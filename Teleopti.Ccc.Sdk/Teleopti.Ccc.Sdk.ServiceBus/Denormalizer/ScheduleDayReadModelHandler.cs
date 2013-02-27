using Rhino.ServiceBus;
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
	public class ScheduleDayReadModelHandler : ConsumerOf<DenormalizedSchedule>, ConsumerOf<DenormalizedScheduleForScheduleDay>
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleDayReadModelHandler));

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonRepository _personRepository;
		private readonly ISignificantChangeChecker _significantChangeChecker;
		private readonly ISmsLinkChecker _smsLinkChecker;
		private readonly INotificationSenderFactory _notificationSenderFactory;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sms")]
		public ScheduleDayReadModelHandler(IUnitOfWorkFactory unitOfWorkFactory,
		                           IPersonRepository personRepository,
		                           ISignificantChangeChecker significantChangeChecker, 
								   ISmsLinkChecker smsLinkChecker,
								   INotificationSenderFactory notificationSenderFactory,
									IScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
									IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personRepository = personRepository;
			_significantChangeChecker = significantChangeChecker;
			_smsLinkChecker = smsLinkChecker;
			_notificationSenderFactory = notificationSenderFactory;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizedSchedule message)
		{
			createReadModel(message);
		}

		private void createReadModel(DenormalizedScheduleBase message)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (!message.IsDefaultScenario) return;

				var date = new DateOnly(message.Date);
				var person = _personRepository.Get(message.PersonId);
				var dateOnlyPeriod = new DateOnlyPeriod(date, date);

				var readModel = _scheduleDayReadModelsCreator.GetReadModels(message);

				if (DefinedLicenseDataFactory.LicenseActivator == null)
				{
					if (Logger.IsInfoEnabled)
						Logger.Info("Can't access LicenseActivator to check SMSLink license.");
					return;
				}

				if (Logger.IsInfoEnabled)
					Logger.Info("Checking SMSLink license.");
				//check for SMS license, if none just skip this. Later we maybe have to check against for example EMAIL-license
				if (
					DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Contains(
						DefinedLicenseOptionPaths.TeleoptiCccSmsLink))
				{
					var smsMessages = _significantChangeChecker.SignificantChangeNotificationMessage(date, person, readModel);
					if (!string.IsNullOrEmpty(smsMessages.Subject))
					{
						var number = _smsLinkChecker.SmsMobileNumber(person);
						if (!string.IsNullOrEmpty(number))
						{
							var smsSender = _notificationSenderFactory.GetSender();
							smsSender.SendNotification(smsMessages, number);
						}
					}
				}

				if (!message.IsInitialLoad)
				{
					_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
				}
				_scheduleDayReadModelRepository.SaveReadModel(readModel);

				uow.PersistAll();
			}
		}

		public void Consume(DenormalizedScheduleForScheduleDay message)
		{
			createReadModel(message);
		}
	}
}