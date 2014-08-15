using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class CalculateBadgeConsumer : ConsumerOf<CalculateBadgeMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IAgentBadgeSettingsRepository _settingsRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IGlobalSettingDataRepository _globalSettingRep;
		private readonly IPushMessagePersister _msgPersister;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IAgentBadgeCalculator _calculator;
		private readonly INow _now;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CalculateBadgeConsumer));

		public CalculateBadgeConsumer(
									IServiceBus serviceBus, 
									IAgentBadgeSettingsRepository settingsRepository, 
									IPersonRepository personRepository, 
									IGlobalSettingDataRepository globalSettingRep,
									IPushMessagePersister msgPersister, 
									ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
									IAgentBadgeCalculator calculator,
									INow now)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_personRepository = personRepository;
			_globalSettingRep = globalSettingRep;
			_msgPersister = msgPersister;
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
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Consume CalculateBadgeMessage with BusinessUnit {0}, DataSource {1} and timezone {2}", message.BusinessUnitId,
					message.Datasource, message.TimeZoneCode);
			}

			var today = _now.LocalDateOnly();
			var tomorrow = today.AddDays(1);
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);
			var tomorrowForGivenTimeZone = TimeZoneInfo.ConvertTime(tomorrow, TimeZoneInfo.Local, timeZone);

			// Set badge calculation start at 5:00 AM
			// Just hard code it now, the best solution is to trigger it from ETL
			var nextMessageShouldBeProcessed =
				TimeZoneInfo.ConvertTime(tomorrowForGivenTimeZone.Date, timeZone, TimeZoneInfo.Local).AddHours(5);
			var peopleGotABadge = new List<IPerson>();

			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var setting = _settingsRepository.LoadAll().FirstOrDefault();
				if (setting == null)
				{
					//error happens
					Logger.Error("Agent badge threshold setting is null before starting badge calculation");
					return;
				}
				var adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());

				var allAgents = _personRepository.FindPeopleInOrganization(new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1)), false);

				peopleGotABadge = _calculator.Calculate(allAgents, message.TimeZoneCode, new DateOnly(message.CalculationDate),
					adherenceReportSetting.CalculationMethod, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate).ToList();

				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Total {0} agents will get new badge", peopleGotABadge.Count());
				}

			}
			//For some reason, the session above cant persist the message since the session changed somehow. Dont know why.
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				foreach (var person in peopleGotABadge)
				{
					if (Logger.IsDebugEnabled)
					{
						Logger.DebugFormat("Send badge message to agent {0} (ID: {1})", person.Name, person.Id);
					}

					SendPushMessageService
						.CreateConversation(Resources.Congratulations, Resources.YouGotNewBadges, false)
						.To(person)
						.SendConversation(_msgPersister);
				}

				uow.PersistAll();
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

			if (Logger.IsDebugEnabled)
			{			Logger.DebugFormat(
						"Delay Sending CalculateBadgeMessage to Service Bus for Timezone={0} on next calculation time={1}", message.TimeZoneCode,
						nextMessageShouldBeProcessed);
			}
		}
	}
}