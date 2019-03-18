using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
			var adjustments = _eventStore.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>
				(
					new DateTimePeriod(DateTime.MinValue, DateTime.MaxValue)
				)
				.Cast<IRtaStoredEvent>()
				.Select(x => new OpenPeriod(x.QueryData().StartTime, x.QueryData().EndTime))
				.ToList();
			
			var cancellations = _eventStore.LoadOfTypeForPeriod<PeriodAdjustmentToNeutralCanceledEvent>
			(
				new DateTimePeriod(DateTime.MinValue, DateTime.MaxValue)
			)
				.Cast<IRtaStoredEvent>()
				.Select(x => new OpenPeriod(x.QueryData().StartTime, x.QueryData().EndTime))
				.ToArray();

			cancellations.ForEach(x => adjustments.Remove(x));
			
			return adjustments
				.Select(e => new AdjustedPeriodViewModel
					{
						StartTime = formatForUser(e.StartTime),
						EndTime = formatForUser(e.EndTime)
					}
				).ToArray();
		}

		private string formatForUser(DateTime? time) =>
			TimeZoneInfo.ConvertTimeFromUtc(time.Value, _timezone.TimeZone()).ToString("yyyy-MM-dd HH\\:mm");
	}
}