using System.Collections.Generic;
using Teleopti.Wfm.Adherence.Historical.Events;

namespace Teleopti.Wfm.Adherence.Historical.Adjustments
{
	public class Adjustments
	{
		public IEnumerable<OpenPeriod> AdjustedPeriods() => _collectedAdjustedToNeutralPeriods;

		private readonly List<OpenPeriod> _collectedAdjustedToNeutralPeriods = new List<OpenPeriod>();

		public void Apply(PeriodAdjustedToNeutralEvent @event) =>
			_collectedAdjustedToNeutralPeriods.Add(new OpenPeriod {StartTime = @event.StartTime, EndTime = @event.EndTime});

		public void Apply(PeriodAdjustmentToNeutralCanceledEvent @event) =>
			_collectedAdjustedToNeutralPeriods.Remove(new OpenPeriod {StartTime = @event.StartTime, EndTime = @event.EndTime});
	}
}