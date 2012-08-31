﻿using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ScheduleDayReadModelHandler : ConsumerOf<DenormalizeScheduleProjection>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ISignificantChangeChecker _significantChangeChecker;
		private readonly ISmsLinkChecker _smsLinkChecker;
		private readonly INotificationSenderFactory _notificationSenderFactory;
		private readonly IScheduleDayReadModelsCreator _scheduleDayReadModelsCreator;
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sms")]
		public ScheduleDayReadModelHandler(IUnitOfWorkFactory unitOfWorkFactory, IScenarioRepository scenarioRepository,
		                           IPersonRepository personRepository,
		                           ISignificantChangeChecker significantChangeChecker, 
								   ISmsLinkChecker smsLinkChecker,
								   INotificationSenderFactory notificationSenderFactory,
									IScheduleDayReadModelsCreator scheduleDayReadModelsCreator,
									IScheduleDayReadModelRepository scheduleDayReadModelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_significantChangeChecker = significantChangeChecker;
			_smsLinkChecker = smsLinkChecker;
			_notificationSenderFactory = notificationSenderFactory;
			_scheduleDayReadModelsCreator = scheduleDayReadModelsCreator;
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizeScheduleProjection message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				if (!scenario.DefaultScenario) return;

				var period = new DateTimePeriod(message.StartDateTime, message.EndDateTime);
				var person = _personRepository.Get(message.PersonId);
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);

				var newReadModels = _scheduleDayReadModelsCreator.GetReadModels(scenario, period, person);
				//check for SMS license, if none just skip this. Later we maybe have to check agians for example EMAIL-license
				if (DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Contains("TeleoptiCcc/SMSLink"))
				{
					var smsMessages = _significantChangeChecker.SignificantChangeNotificationMessage(dateOnlyPeriod, person, newReadModels);
					if (!string.IsNullOrEmpty(smsMessages.Subject))
					{
						var number = _smsLinkChecker.SmsMobileNumber(person);
						if (!string.IsNullOrEmpty(number))
						{
							var smsSender = _notificationSenderFactory.Sender;
							smsSender.SendNotification(smsMessages, number);
						}
					}
				}
				//do update of read model
				_scheduleDayReadModelRepository.ClearPeriodForPerson(dateOnlyPeriod, message.PersonId);
				_scheduleDayReadModelRepository.SaveReadModels(newReadModels);
			}
		}
	}
}