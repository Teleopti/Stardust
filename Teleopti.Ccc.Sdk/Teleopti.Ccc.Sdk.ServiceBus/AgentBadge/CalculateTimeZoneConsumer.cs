using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class CalculateTimeZoneConsumer : ConsumerOf<CalculateTimeZoneMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly INow _now;
		private static readonly ILog logger = LogManager.GetLogger(typeof(CalculateTimeZoneConsumer));

		public CalculateTimeZoneConsumer(IServiceBus serviceBus, INow now)
		{
			_serviceBus = serviceBus;
			_now = now;
		}

		/// <summary>
		/// Get the date to calculate for the given timezone
		/// Send CalculateBadgeMessage to bus for time of next calculation
		/// </summary>
		/// <param name="message"></param>
		public void Consume(CalculateTimeZoneMessage message)
		{
			const int badgeCalculationDelayDays = -2;

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consume CalculateTimeZoneMessage with BusinessUnit {0}, DataSource {1} and timezone {2}",
					message.LogOnBusinessUnitId, message.LogOnDatasource, message.TimeZoneCode);
			}
			var today = _now.LocalDateTime();
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);
			var todayForGivenTimeZone = TimeZoneInfo.ConvertTime(today, TimeZoneInfo.Local, timeZone);
			var calculationDateForGivenTimeZone = todayForGivenTimeZone.AddDays(badgeCalculationDelayDays).Date;

			_serviceBus.Send(new CalculateBadgeMessage
			{
				LogOnDatasource = message.LogOnDatasource,
				LogOnBusinessUnitId = message.LogOnBusinessUnitId,
				Timestamp = DateTime.UtcNow,
				CalculationDate = calculationDateForGivenTimeZone,
				TimeZoneCode = message.TimeZoneCode 
			});

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Sending CalculateBadgeMessage to Service Bus for Timezone={0} on calculation date={1:yyyy-MM-dd HH:mm:ss}",
					message.TimeZoneCode, calculationDateForGivenTimeZone.Date);
			}
		}
	}
}