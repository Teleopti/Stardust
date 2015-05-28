using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IPlanningPeriodSuggestions
	{
		SuggestedPlanningPeriod Default();
		IEnumerable<SuggestedPlanningPeriod> SuggestedPeriods(DateOnlyPeriod forDate);
	}

	public struct SuggestedPlanningPeriod : IEquatable<SuggestedPlanningPeriod>
	{
		public SchedulePeriodType PeriodType { get; set; }
		public int Number { get; set; }
		public DateOnlyPeriod Range { get; set; }

		public bool Equals(SuggestedPlanningPeriod other)
		{
			return PeriodType == other.PeriodType && Number == other.Number && Range == other.Range;
		}

		public override int GetHashCode()
		{
			return PeriodType.GetHashCode() ^ Number.GetHashCode() ^ Range.GetHashCode();
		}
	}
}