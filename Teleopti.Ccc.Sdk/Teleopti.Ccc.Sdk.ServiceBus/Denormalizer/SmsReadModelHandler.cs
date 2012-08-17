using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer.SMS;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class SmsReadModelHandler : ConsumerOf<DenormalizeScheduleProjection>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ISignificantChangeChecker _significantChangeChecker;
		private readonly ISmsLinkChecker _smsLinkChecker;
		private readonly ISmsSender _smsSender;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sms")]
		public SmsReadModelHandler(IUnitOfWorkFactory unitOfWorkFactory, IScenarioRepository scenarioRepository,
		                           IPersonRepository personRepository,
		                           ISignificantChangeChecker significantChangeChecker, ISmsLinkChecker smsLinkChecker,
		                           ISmsSender smsSender)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_significantChangeChecker = significantChangeChecker;
			_smsLinkChecker = smsLinkChecker;
			_smsSender = smsSender;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizeScheduleProjection message)
		{
			//check for license, if none just skip this
			if (!DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Contains("TeleoptiCcc/SMSLink")) return;
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var scenario = _scenarioRepository.Get(message.ScenarioId);
				if (!scenario.DefaultScenario) return;

				var period = new DateTimePeriod(message.StartDateTime, message.EndDateTime);
				var person = _personRepository.Get(message.PersonId);
				var timeZone = person.PermissionInformation.DefaultTimeZone();
				var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);

				if (_significantChangeChecker.IsSignificantChange(dateOnlyPeriod, person))
				{
					var number = _smsLinkChecker.SmsMobileNumber(person);
					if (!string.IsNullOrEmpty(number))
						_smsSender.SendSms(dateOnlyPeriod, number);
				}

				//if (message.SkipDelete)
				//{
				//    _updateScheduleProjectionReadModel.SetInitialLoad(true);
				//}

				//do update of read model later
			}
		}
	}
}