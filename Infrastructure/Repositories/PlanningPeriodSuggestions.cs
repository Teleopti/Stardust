using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningPeriodSuggestions : IPlanningPeriodSuggestions
	{
		private readonly INow _now;
		private readonly IList<AggregatedSchedulePeriod> _uniqueSchedulePeriods;
		private readonly SchedulePeriodRangeCalculator _schedulePeriodRangeCalculator = new SchedulePeriodRangeCalculator();

		public PlanningPeriodSuggestions(INow now, IList<AggregatedSchedulePeriod> uniqueSchedulePeriods)
		{
			_now = now;
			_uniqueSchedulePeriods = uniqueSchedulePeriods;
		}

		public DateOnlyPeriod Default(bool isToggleEnabled)
		{
			if (_uniqueSchedulePeriods.Any() && isToggleEnabled)
			{
				var aggregatedSchedulePeriod =  _uniqueSchedulePeriods.OrderByDescending(x => x.Priority).First();
				var currentPeriod =  _schedulePeriodRangeCalculator.PeriodForType(new DateOnly(_now.UtcDateTime()),
					rangeCalculation(aggregatedSchedulePeriod));

				return _schedulePeriodRangeCalculator.PeriodForType(new DateOnly(currentPeriod.EndDate.AddDays(1).Date),
					rangeCalculation(aggregatedSchedulePeriod));
			}
			return monthIsLastResort();
		}

		public IEnumerable<SchedulePeriodType> UniqueSuggestedPeriod { get
		{
			return _uniqueSchedulePeriods.Select(x => x.PeriodType);
		}  }

		private static SchedulePeriodForRangeCalculation rangeCalculation(AggregatedSchedulePeriod aggregatedSchedulePeriod)
		{
			return new SchedulePeriodForRangeCalculation()
			{
				Culture = CultureInfo.CurrentCulture,
				PeriodType = aggregatedSchedulePeriod.PeriodType,
				Number = aggregatedSchedulePeriod.Number,
				StartDate = new DateOnly(aggregatedSchedulePeriod.DateFrom)
			};
		}

		private DateOnlyPeriod monthIsLastResort()
		{
			var date = _now.LocalDateTime();
			var firstDateOfMonth = DateHelper.GetLastDateInMonth(date, CultureInfo.CurrentCulture).AddDays(1);
			var lastDateOfMonth = DateHelper.GetLastDateInMonth(firstDateOfMonth, CultureInfo.CurrentCulture);
			return new DateOnlyPeriod(new DateOnly(firstDateOfMonth), new DateOnly(lastDateOfMonth));
		}
	}
}