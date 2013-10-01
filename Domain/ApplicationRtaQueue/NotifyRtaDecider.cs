using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationRtaQueue
{
	public static class NotifyRtaDecider
	{
		public static bool ShouldSendMessage(DateTimePeriod period, DateTime? nearestLayerStartDateTime)
		{
			return (period.StartDateTime == DateTime.MaxValue.ToUniversalTime()
					&& period.EndDateTime == DateTime.MaxValue.ToUniversalTime())
			       ||
			       (nearestLayerStartDateTime != null &&
			        (DateTime.UtcNow < period.EndDateTime &&
			         nearestLayerStartDateTime.Value.AddDays(3) > period.StartDateTime &&
			         nearestLayerStartDateTime.Value.AddDays(-2) < period.EndDateTime))
			       ||
			       (nearestLayerStartDateTime == null &&
			        period.EndDateTime > DateTime.UtcNow);
		}
	}
}
