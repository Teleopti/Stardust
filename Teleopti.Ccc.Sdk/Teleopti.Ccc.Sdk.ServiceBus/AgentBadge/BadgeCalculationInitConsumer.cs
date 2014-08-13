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
		private static readonly ILog Logger = LogManager.GetLogger(typeof(BadgeCalculationInitConsumer));


		/// <summary>
		/// Get all timezones using businessunitrepository
		/// Foreach send CalculateTimeZoneMessage
		/// TODO:existed problem: new added timezone to the system will not be calculated now.
		/// </summary>
		/// <param name="serviceBus"></param>
		/// <param name="settingsRepository"></param>
		/// <param name="buRepository"></param>
		/// <param name="unitOfWorkFactory"></param>
		public BadgeCalculationInitConsumer(IServiceBus serviceBus, IAgentBadgeSettingsRepository settingsRepository,
			IBusinessUnitRepository buRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_buRepository = buRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Consume(BadgeCalculationInitMessage message)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Consume message with BusinessUnit {0} and DataSource {1}", message.BusinessUnitId,
					message.Datasource);
			}

			if (_serviceBus == null)
			{
				Logger.Error("Service bus instance is null for some reason!");
				return;
			}

			List<TimeZoneInfo> timeZoneList;
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var setting = _settingsRepository.LoadAll().FirstOrDefault();
				if (setting == null || !setting.EnableBadge)
				{
					_serviceBus.DelaySend(DateOnly.Today.AddDays(1), message);
					if (Logger.IsDebugEnabled)
					{
						Logger.DebugFormat(
							"Feature is diabled. Delay Sending BadgeCalculationInitMessage to Service Bus for "
							+ "BusinessUnitId={0} in DataSource={1} tommorrow", message.BusinessUnitId,
							message.Datasource);
					}
					return;
				}

				timeZoneList = _buRepository.LoadAllTimeZones().ToList();
			}
			
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Retrieved {0} timezones to calculage badges.", timeZoneList.Count());
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

				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat(
						"Sending CalculateTimeZoneMessage to Service Bus for Timezone={0} in BusinessUnitId={1}",
						timeZoneInfo.Id, message.BusinessUnitId);
				}
			}
		}
	}
}