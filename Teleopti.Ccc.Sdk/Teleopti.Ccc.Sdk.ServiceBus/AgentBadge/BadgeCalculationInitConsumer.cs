using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class BadgeCalculationInitConsumer : ConsumerOf<BadgeCalculationInitMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IAgentBadgeSettingsRepository _settingsRepository;
		private readonly IBusinessUnitRepository _buRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CalculateTimeZoneConsumer));

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

		public void Consume(BadgeCalculationInitMessage message)
		{
			if (_serviceBus == null)
			{
				Logger.Error("service bus instance is null for some reason!");
				return;
			}
				
			IEnumerable<TimeZoneInfo> timeZoneList;
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				IAgentBadgeThresholdSettings setting = _settingsRepository.LoadAll().FirstOrDefault();
				if (setting == null || !setting.EnableBadge)
				{
					_serviceBus.DelaySend(DateOnly.Today.AddDays(1), message);
					Logger.DebugFormat(
						"Feature is diabled. Delay Sending BadgeCalculationInitMessage to Service Bus for BusinessUnitId={0} in DataSource={1} tommorrow", message.BusinessUnitId,
						message.Datasource);
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
					TimeZoneCode = timeZoneInfo.Id
				});
				Logger.DebugFormat(
						"Sending CalculateTimeZoneMessage to Service Bus for Timezone={0} in BusinessUnitId={1}", timeZoneInfo.Id,
						message.BusinessUnitId);
			}
		}
	}
}