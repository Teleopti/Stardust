using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationRtaQueue
{
	public static class NotifyRtaDecider
	{
		public static bool ShouldSendMessage(DateTimePeriod period, DateTime nearestLayerStartDateTime)
		{
			return DateTime.UtcNow < period.EndDateTime &&
			       nearestLayerStartDateTime.AddDays(3) > period.StartDateTime &&
				   nearestLayerStartDateTime.AddDays(-2) < period.EndDateTime;
		}

	}
}
