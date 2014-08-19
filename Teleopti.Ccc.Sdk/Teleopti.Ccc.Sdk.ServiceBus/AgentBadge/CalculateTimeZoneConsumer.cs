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
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CalculateTimeZoneConsumer));

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
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Consume CalculateTimeZoneMessage with BusinessUnit {0}, DataSource {1} and timezone {2}",
					message.BusinessUnitId, message.Datasource, message.TimeZoneCode);
			}
			var today = _now.LocalDateTime();
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);
			var todayForGivenTimeZone = TimeZoneInfo.ConvertTime(today, TimeZoneInfo.Local, timeZone);
			var yesterdayForGivenTimeZone = todayForGivenTimeZone.AddDays(-1);

			_serviceBus.Send(new CalculateBadgeMessage
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				Timestamp = DateTime.UtcNow,
				CalculationDate = yesterdayForGivenTimeZone.Date,
				TimeZoneCode = message.TimeZoneCode 
			});

			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat(
					"Sending CalculateBadgeMessage to Service Bus for Timezone={0} on calculation date={1:yyyy-MM-dd HH:mm:ss}",
					message.TimeZoneCode, yesterdayForGivenTimeZone.Date);
			}
		}
	}
}