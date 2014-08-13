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
		//get the date to calculate for the given timezone
		//send CalculateBadgeMessage to bus for time of next calculation
		public CalculateTimeZoneConsumer(IServiceBus serviceBus, INow now)
		{
			_serviceBus = serviceBus;
			_now = now;
		}

		public void Consume(CalculateTimeZoneMessage message)
		{
			var yesterday = _now.LocalDateOnly().AddDays(-1);
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);
			var yesterdayForGivenTimeZone = TimeZoneInfo.ConvertTime(yesterday, TimeZoneInfo.Local, timeZone);

			_serviceBus.Send(new CalculateBadgeMessage
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				Timestamp = DateTime.UtcNow,
				CalculationDate = yesterdayForGivenTimeZone,
				TimeZoneCode = message.TimeZoneCode 
			});
			Logger.DebugFormat(
						"Sending CalculateBadgeMessage to Service Bus for Timezone={0} on calculation time={1}", message.TimeZoneCode,
						yesterdayForGivenTimeZone);
		}
	}
}