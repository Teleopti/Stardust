using System;
using Rhino.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class CalculateTimeZoneConsumer : ConsumerOf<CalculateTimeZoneMessage>
	{
		private readonly IServiceBus _serviceBus;
		//get the date to calculate for the given timezone
		//send CalculateBadgeMessage to bus for time of next calculation
		public CalculateTimeZoneConsumer(IServiceBus serviceBus)
		{
			_serviceBus = serviceBus;
		}

		public void Consume(CalculateTimeZoneMessage message)
		{
			var yesterday = DateOnly.Today.AddDays(-1);
			var yesterdayForGivenTimeZone = TimeZoneInfo.ConvertTime(yesterday, TimeZoneInfo.Local, message.TimeZone);

			_serviceBus.Send(new CalculateBadgeMessage
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				Timestamp = DateTime.UtcNow,
				CalculationDate = new DateOnly(yesterdayForGivenTimeZone),
				TimeZone = message.TimeZone 
			});
		}
	}
}