using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces;
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

		public SuggestedPlanningPeriod Default()
		{
			var now = _now.LocalDateOnly();
			if (_uniqueSchedulePeriods.Any() )
			{
				var aggregatedSchedulePeriod = _uniqueSchedulePeriods.First();
				var currentPeriod = _schedulePeriodRangeCalculator.PeriodForType(now,
					rangeCalculation(aggregatedSchedulePeriod));

				return new SuggestedPlanningPeriod
				{
					Number = aggregatedSchedulePeriod.Number,
					PeriodType = aggregatedSchedulePeriod.PeriodType,
					Range = _schedulePeriodRangeCalculator.PeriodForType(currentPeriod.EndDate.AddDays(1),
						rangeCalculation(aggregatedSchedulePeriod))
				};
			}
			return monthIsLastResort(now.Date);
		}

		public IEnumerable<SuggestedPlanningPeriod> SuggestedPeriods(DateOnly forDate)
		{
			var result = new List<SuggestedPlanningPeriod>();

			var resultingRanges = _uniqueSchedulePeriods.SelectMany(uniqueSchedulePeriod => new []{ new SuggestedPlanningPeriod
			{
				PeriodType = uniqueSchedulePeriod.PeriodType,
				Number = uniqueSchedulePeriod.Number,
				Range =
					_schedulePeriodRangeCalculator.PeriodForType(forDate,
						new SchedulePeriodForRangeCalculation
						{
							Culture =
								CultureInfo.GetCultureInfo(uniqueSchedulePeriod.Culture.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)),
							Number = uniqueSchedulePeriod.Number,
							PeriodType = uniqueSchedulePeriod.PeriodType,
							StartDate = new DateOnly(uniqueSchedulePeriod.DateFrom)
						})
			},new SuggestedPlanningPeriod
			{
				PeriodType = uniqueSchedulePeriod.PeriodType,
				Number = uniqueSchedulePeriod.Number * 2,
				Range =
					_schedulePeriodRangeCalculator.PeriodForType(forDate,
						new SchedulePeriodForRangeCalculation
						{
							Culture =
								CultureInfo.GetCultureInfo(uniqueSchedulePeriod.Culture.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)),
							Number = uniqueSchedulePeriod.Number * 2,
							PeriodType = uniqueSchedulePeriod.PeriodType,
							StartDate = new DateOnly(uniqueSchedulePeriod.DateFrom)
						})
			}});

			result.AddRange(resultingRanges.Distinct());
			result.Add(monthIsLastResort(forDate.Date));
			return result;
		}

		private static SchedulePeriodForRangeCalculation rangeCalculation(AggregatedSchedulePeriod aggregatedSchedulePeriod)
		{
			return new SchedulePeriodForRangeCalculation
			{
				Culture = CultureInfo.CurrentCulture,
				PeriodType = aggregatedSchedulePeriod.PeriodType,
				Number = aggregatedSchedulePeriod.Number,
				StartDate = new DateOnly(aggregatedSchedulePeriod.DateFrom)
			};
		}

		private SuggestedPlanningPeriod monthIsLastResort(DateTime givenDate)
		{
			var firstDateOfMonth = DateHelper.GetLastDateInMonth(givenDate, CultureInfo.CurrentCulture).AddDays(1);
			var lastDateOfMonth = DateHelper.GetLastDateInMonth(firstDateOfMonth, CultureInfo.CurrentCulture);
			return new SuggestedPlanningPeriod{PeriodType = SchedulePeriodType.Month,Number = 1,Range = new DateOnlyPeriod(new DateOnly(firstDateOfMonth), new DateOnly(lastDateOfMonth))};
		}
	}
	
}