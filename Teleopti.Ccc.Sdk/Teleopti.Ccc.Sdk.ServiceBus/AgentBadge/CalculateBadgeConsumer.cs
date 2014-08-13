using System;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class CalculateBadgeConsumer : ConsumerOf<CalculateBadgeMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IAgentBadgeSettingsRepository _settingsRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IGlobalSettingDataRepository _globalSettingRep;
		private readonly IPushMessageRepository _msgRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IAgentBadgeCalculator _calculator;
		private readonly INow _now;

		public CalculateBadgeConsumer(
									IServiceBus serviceBus, 
									IAgentBadgeSettingsRepository settingsRepository, 
									IPersonRepository personRepository, 
									IGlobalSettingDataRepository globalSettingRep, 
									IPushMessageRepository msgRepository, 
									ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
									IAgentBadgeCalculator calculator,
									INow now)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_personRepository = personRepository;
			_globalSettingRep = globalSettingRep;
			_msgRepository = msgRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_calculator = calculator;
			_now = now;
		}

		/// <summary>
		/// Calculate the badge stuff
		/// Get the date for doing the next calculation
		/// Delaysend CalculateBadgeMessage to bus for time of next calculation
		/// </summary>
		/// <param name="message"></param>
		public void Consume(CalculateBadgeMessage message)
		{
			var today = _now.LocalDateOnly();
			var tomorrow = today.AddDays(1);
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);
			var tomorrowForGivenTimeZone = TimeZoneInfo.ConvertTime(tomorrow, TimeZoneInfo.Local, timeZone);

			// Set badge calculation start at 5:00 AM
			// Just hard code it now, the best solution is to trigger it from ETL
			var nextMessageShouldBeProcessed =
				TimeZoneInfo.ConvertTime(tomorrowForGivenTimeZone.Date, timeZone, TimeZoneInfo.Local).AddHours(5);

			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var setting = _settingsRepository.LoadAll().FirstOrDefault();
				if (setting == null)
				{
					//TODO:error
					return;
				}
				var adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());
				
				var allAgents = _personRepository.FindPeopleInOrganization(new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1)), false);

				var peopleGotABadge = _calculator.Calculate(allAgents, message.TimeZoneCode, new DateOnly(message.CalculationDate),
					adherenceReportSetting.CalculationMethod, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
				foreach (var person in peopleGotABadge)
				{
					SendPushMessageService
						.CreateConversation(Resources.Congratulations, Resources.YouGotNewBadges, false)
						.To(person)
						.SendConversation(_msgRepository);
					uow.PersistAll();
				}
			}

			if (_serviceBus == null) return;

			_serviceBus.DelaySend(nextMessageShouldBeProcessed, new CalculateBadgeMessage
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				Timestamp = DateTime.UtcNow,
				TimeZoneCode = message.TimeZoneCode,
				CalculationDate = message.CalculationDate.AddDays(1)
			});
		}
	}
}