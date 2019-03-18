using System;
using System.Linq;
using Teleopti.Wfm.Adherence.Historical.Events;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;

namespace Teleopti.Wfm.Adherence.Historical.Adjustments
{
	public class AdjustmentsLoader
	{
		private readonly IRtaEventStoreReader _eventStore;

		public AdjustmentsLoader(IRtaEventStoreReader eventStore)
		{
			_eventStore = eventStore;
		}

		public Adjustments Load()
		{
			var period = new DateTimePeriod(DateTime.MinValue, DateTime.MaxValue);
			var events = _eventStore.LoadOfTypeForPeriod<PeriodAdjustedToNeutralEvent>(period)
				.Concat(_eventStore.LoadOfTypeForPeriod<PeriodAdjustmentToNeutralCanceledEvent>(period));

			var saga = new Adjustments();
			events.ForEach(x =>
			{
				var method = saga.GetType().GetMethod("Apply", new[] {x.GetType()});
				if (method != null)
					saga.Apply((dynamic) x);
			});
			return saga;
		}
	}
}