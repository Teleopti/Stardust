using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

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
			var now = new DateOnly(_now.UtcDateTime());
			if (_uniqueSchedulePeriods.Any() )
			{
				var aggregatedSchedulePeriod = _uniqueSchedulePeriods.First();
				if (aggregatedSchedulePeriod.DateFrom > now.Date)
				{
					return new SuggestedPlanningPeriod
					{
						Number = aggregatedSchedulePeriod.Number,
						PeriodType = aggregatedSchedulePeriod.PeriodType,
						Range = _schedulePeriodRangeCalculator.PeriodForType(new DateOnly(aggregatedSchedulePeriod.DateFrom),
							rangeCalculation(aggregatedSchedulePeriod))
					};
				}
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

		public IEnumerable<SuggestedPlanningPeriod> SuggestedPeriods(DateOnly startDate)
		{
			var result = new List<Tuple<int, SuggestedPlanningPeriod>>();
			var resultingRanges = _uniqueSchedulePeriods.SelectMany(uniqueSchedulePeriod =>
			{
				var innerResult = new List<Tuple<int, SuggestedPlanningPeriod>>();
				
				var rangeForPeriod = new SchedulePeriodForRangeCalculation
				{
					Culture =
						CultureInfo.GetCultureInfo(uniqueSchedulePeriod.Culture.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)),
					Number = uniqueSchedulePeriod.Number,
					PeriodType = uniqueSchedulePeriod.PeriodType,
					StartDate = new DateOnly(uniqueSchedulePeriod.DateFrom)
				};

				DateOnlyPeriod firstSingleRange;
				if (uniqueSchedulePeriod.DateFrom > startDate.Date)
				{
					firstSingleRange = _schedulePeriodRangeCalculator.PeriodForType(new DateOnly(uniqueSchedulePeriod.DateFrom), rangeForPeriod);
				}
				else
				{
					var periodContainingStartDate = _schedulePeriodRangeCalculator.PeriodForType(startDate, rangeForPeriod);
					firstSingleRange = _schedulePeriodRangeCalculator.PeriodForType(periodContainingStartDate.EndDate.AddDays(1), rangeForPeriod);
				}

				var secondSingleRange = _schedulePeriodRangeCalculator.PeriodForType(firstSingleRange.EndDate.AddDays(1), rangeForPeriod);
				var firstDoubleRange = new DateOnlyPeriod(firstSingleRange.StartDate, secondSingleRange.EndDate);

				innerResult.Add(new Tuple<int, SuggestedPlanningPeriod>(uniqueSchedulePeriod.Priority, new SuggestedPlanningPeriod
				{
					PeriodType = uniqueSchedulePeriod.PeriodType,
					Number = uniqueSchedulePeriod.Number,
					Range = firstSingleRange
				}));
				innerResult.Add(new Tuple<int, SuggestedPlanningPeriod>(uniqueSchedulePeriod.Priority, new SuggestedPlanningPeriod
				{
					PeriodType = uniqueSchedulePeriod.PeriodType,
					Number = uniqueSchedulePeriod.Number,
					Range = secondSingleRange
				}));
				innerResult.Add(new Tuple<int, SuggestedPlanningPeriod>(uniqueSchedulePeriod.Priority, new SuggestedPlanningPeriod
				{
					PeriodType = uniqueSchedulePeriod.PeriodType,
					Number = uniqueSchedulePeriod.Number * 2,
					Range = firstDoubleRange
				}));
				return innerResult;
			});
			result.AddRange(resultingRanges);
			return
				result.Where(r => r.Item2.Range.StartDate > new DateOnly(_now.UtcDateTime()))
					.GroupBy(i => i.Item2)
					.Select(s => new { s.Key, Score = s.Sum(v => v.Item1) })
					.OrderByDescending(x => x.Score)
					.ThenBy(y => y.Key.Range.StartDate)
					.Select(z => z.Key)
					.ToArray();
		}

		public IEnumerable<SuggestedPlanningPeriod> SuggestedPeriods(DateOnlyPeriod range)
		{
			var result = new List<Tuple<int, SuggestedPlanningPeriod>>();
			
			var resultingRanges = _uniqueSchedulePeriods.SelectMany(uniqueSchedulePeriod => 
			{
				var innerResult = new List<Tuple<int,SuggestedPlanningPeriod>>();
				var currentDate = range.StartDate;
				while (currentDate <= range.EndDate)
				{
					var rangeForPeriod = new SchedulePeriodForRangeCalculation
					{
						Culture =
							CultureInfo.GetCultureInfo(uniqueSchedulePeriod.Culture.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)),
						Number = uniqueSchedulePeriod.Number,
						PeriodType = uniqueSchedulePeriod.PeriodType,
						StartDate = new DateOnly(uniqueSchedulePeriod.DateFrom)
					};
					var singlePeriod = new Tuple<int,SuggestedPlanningPeriod>(uniqueSchedulePeriod.Priority,new SuggestedPlanningPeriod
					{
						PeriodType = uniqueSchedulePeriod.PeriodType,
						Number = uniqueSchedulePeriod.Number,
						Range = _schedulePeriodRangeCalculator.PeriodForType(currentDate, rangeForPeriod)
					});

					currentDate = singlePeriod.Item2.Range.EndDate.AddDays(1);
					var rangeForDoublePeriod = new SchedulePeriodForRangeCalculation
					{
						Culture =
							CultureInfo.GetCultureInfo(uniqueSchedulePeriod.Culture.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)),
						Number = uniqueSchedulePeriod.Number,
						PeriodType = uniqueSchedulePeriod.PeriodType,
						StartDate = new DateOnly(uniqueSchedulePeriod.DateFrom)
					};
					var doublePeriod =new Tuple<int, SuggestedPlanningPeriod>(uniqueSchedulePeriod.Priority, new SuggestedPlanningPeriod
					{
						PeriodType = uniqueSchedulePeriod.PeriodType,
						Number = uniqueSchedulePeriod.Number*2,
						Range = new DateOnlyPeriod(singlePeriod.Item2.Range.StartDate, _schedulePeriodRangeCalculator.PeriodForType(currentDate, rangeForDoublePeriod).EndDate)
					});
					innerResult.Add(singlePeriod);
					innerResult.Add(doublePeriod);

				}
				return innerResult;
			});

			result.AddRange(resultingRanges);
			return
				result.Where(r => r.Item2.Range.StartDate > new DateOnly(_now.UtcDateTime()))
					.GroupBy(i => i.Item2)
					.Select(s => new {s.Key, Score = s.Sum(v => v.Item1)})
					.OrderByDescending(x => x.Score)
					.ThenBy(y => y.Key.Range.StartDate)
					.Select(z => z.Key)
					.ToArray();
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