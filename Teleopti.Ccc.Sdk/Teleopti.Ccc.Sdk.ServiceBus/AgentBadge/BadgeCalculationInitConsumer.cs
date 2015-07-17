using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class BadgeCalculationInitConsumer : ConsumerOf<BadgeCalculationInitMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IGamificationSettingRepository _settingsRepository;
		private readonly IBusinessUnitRepository _buRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private static readonly ILog logger = LogManager.GetLogger(typeof(BadgeCalculationInitConsumer));
		private readonly IToggleManager _toggleManager;

		/// <summary>
		/// Get all timezones using businessunitrepository
		/// Foreach send CalculateTimeZoneMessage
		/// TODO:existed problem: new added timezone to the system will not be calculated now.
		/// </summary>
		/// <param name="serviceBus"></param>
		/// <param name="settingsRepository"></param>
		/// <param name="buRepository"></param>
		/// <param name="unitOfWorkFactory"></param>
		/// <param name="toggleManager"></param>
		public BadgeCalculationInitConsumer(IServiceBus serviceBus, IGamificationSettingRepository settingsRepository,
			IBusinessUnitRepository buRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IToggleManager toggleManager)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_buRepository = buRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_toggleManager = toggleManager;
		}

		public void Consume(BadgeCalculationInitMessage message)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consume message with BusinessUnit {0} and DataSource {1}", message.BusinessUnitId,
					message.Datasource);
			}

			if (_serviceBus == null)
			{
				logger.Error("Service bus instance is null for some reason!");
				return;
			}

			List<TimeZoneInfo> timeZoneList;
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var settings = _settingsRepository.FindAllGamificationSettingsSortedByDescription();
				if (!_toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318))
				{
					if (settings == null || !settings.Any())
					{
						_serviceBus.DelaySend(DateTime.Today.AddDays(1), message);
						if (logger.IsDebugEnabled)
						{
							logger.DebugFormat(
								"Badge not enabled for all teams. Delay Sending BadgeCalculationInitMessage to Service Bus for "
								+ "BusinessUnitId={0} in DataSource={1} tommorrow", message.BusinessUnitId,
								message.Datasource);
						}
						return;
					}
				}

				timeZoneList = _buRepository.LoadAllTimeZones().ToList();
			}
			
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Retrieved {0} timezones to calculage badges.", timeZoneList.Count());
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

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat(
						"Sending CalculateTimeZoneMessage to Service Bus for Timezone={0} in BusinessUnitId={1}",
						timeZoneInfo.Id, message.BusinessUnitId);
				}
			}
		}
	}
}