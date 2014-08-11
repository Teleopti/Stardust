using System;
using System.Collections.Generic;
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
		private readonly IStatisticRepository _statisticRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IGlobalSettingDataRepository _globalSettingRep;
		private readonly IPushMessageRepository _msgRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IAgentBadgeCalculator _calculator;
		private readonly INow _now;

		public CalculateBadgeConsumer(
									IServiceBus serviceBus, 
									IAgentBadgeSettingsRepository settingsRepository,
									IStatisticRepository statisticRepository, 
									IPersonRepository personRepository, 
									IGlobalSettingDataRepository globalSettingRep, 
									IPushMessageRepository msgRepository, 
									ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
									IAgentBadgeCalculator calculator,
									INow now)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_statisticRepository = statisticRepository;
			_personRepository = personRepository;
			_globalSettingRep = globalSettingRep;
			_msgRepository = msgRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_calculator = calculator;
			_now = now;
		}

		public void Consume(CalculateBadgeMessage message)
		{
			//calculate the badge stuff

			//get the date for doing the next calculation
			//delaysend CalculateBadgeMessage to bus for time of next calculation
			//var calculator = new AgentBadgeCalculator(_statisticRepository);
			IAgentBadgeThresholdSettings setting;
			AdherenceReportSetting adherenceReportSetting;
			ICollection<IPerson> allAgents;
			var today = _now.LocalDateOnly();
			var tomorrow = today.AddDays(1);
			var tomorrowForGivenTimeZone = TimeZoneInfo.ConvertTime(tomorrow, TimeZoneInfo.Local, message.TimeZone);
			var nextMessageShouldBeProcessed = TimeZoneInfo.ConvertTime(tomorrowForGivenTimeZone.Date, message.TimeZone,
				TimeZoneInfo.Local);
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				setting = _settingsRepository.LoadAll().FirstOrDefault();
				if (setting == null)
				{
					//TODO:error
					return;
				}
				adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());

				
				allAgents = _personRepository.FindPeopleInOrganization(new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1)), false);

				var peopleGotABadge = _calculator.Calculate(allAgents, message.TimeZone.Id, message.CalculationDate,
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
				TimeZone = message.TimeZone,
				CalculationDate = message.CalculationDate.AddDays(1)
			});

		}
	}
}