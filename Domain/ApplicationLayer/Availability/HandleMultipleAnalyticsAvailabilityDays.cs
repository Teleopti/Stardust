using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Availability
{
	public class HandleMultipleAnalyticsAvailabilityDays
	{
		private readonly HandleOneAnalyticsAvailabilityDay _handleOneAnalyticsAvailabilityDay;

		public HandleMultipleAnalyticsAvailabilityDays(HandleOneAnalyticsAvailabilityDay handleOneAnalyticsAvailabilityDay)
		{
			_handleOneAnalyticsAvailabilityDay = handleOneAnalyticsAvailabilityDay;
		}

		public void Execute(Guid personId, IEnumerable<DateOnly> dates)
		{
			foreach (var date in dates)
			{
				_handleOneAnalyticsAvailabilityDay.Execute(personId, date);
			}
		}

	}
}