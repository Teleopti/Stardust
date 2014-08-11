using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class BadgeCalculationInitConsumer : ConsumerOf<StartUpBusinessUnit>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IAgentBadgeSettingsRepository _settingsRepository;
		private readonly IBusinessUnitRepository _buRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		/// <summary>
		/// get all timezones using businessunitrepository
		/// foreach send CalculateTimeZoneMessage
		/// TODO:existed problem: new added timezone to the system will not be calculated now.
		/// </summary>
		/// <param name="serviceBus"></param>
		/// <param name="settingsRepository"></param>
		/// <param name="buRepository"></param>
		/// <param name="unitOfWorkFactory"></param>
		public BadgeCalculationInitConsumer(IServiceBus serviceBus, IAgentBadgeSettingsRepository settingsRepository, IBusinessUnitRepository buRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_buRepository = buRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Consume(StartUpBusinessUnit message)
		{
			if (_serviceBus == null)
				return;
			IAgentBadgeThresholdSettings setting;
			IEnumerable<TimeZoneInfo> timeZoneList;
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				setting = _settingsRepository.LoadAll().FirstOrDefault();
				if (setting == null || !setting.EnableBadge)
				{
					_serviceBus.DelaySend(DateOnly.Today.AddDays(1), message);
					return;
				}

				timeZoneList = _buRepository.LoadAllTimeZones();
			}
			

			foreach (var timeZoneInfo in timeZoneList)
			{
				_serviceBus.Send(new CalculateTimeZoneMessage
				{
					BusinessUnitId = message.BusinessUnitId,
					Datasource = message.Datasource,
					Timestamp = DateTime.UtcNow,
					TimeZone = timeZoneInfo
				});
			}
		}
	}
}