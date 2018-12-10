using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IPlanningPeriodSuggestions
	{
		SuggestedPlanningPeriod Default();
		IEnumerable<SuggestedPlanningPeriod> SuggestedPeriods(DateOnly startDate);
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