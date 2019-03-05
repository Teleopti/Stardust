using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Wfm.Adherence.Historical
{
	public class AdjustedPeriodsViewModelBuilder
	{
		private readonly IRtaEventStoreReader _eventStore;
		private readonly IUserTimeZone _timezone;

		public AdjustedPeriodsViewModelBuilder(IRtaEventStoreReader eventStore, IUserTimeZone timezone)
		{
			_eventStore = eventStore;
			_timezone = timezone;
		}

		public IEnumerable<AdjustedPeriodViewModel> Build()
		{
			var events = _eventStore.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>
			(
				new DateTimePeriod(DateTime.MinValue, DateTime.MaxValue)
			);

			return events
				.Cast<IRtaStoredEvent>()
				.Select(e => new AdjustedPeriodViewModel
					{
						StartTime = formatForUser(e.QueryData().StartTime),
						EndTime = formatForUser(e.QueryData().EndTime)
					}
				).ToArray();
		}

		private string formatForUser(DateTime? time) => 
			TimeZoneInfo.ConvertTimeFromUtc(time.Value, _timezone.TimeZone()).ToString("yyyy-MM-dd HH\\:mm");
	}
}