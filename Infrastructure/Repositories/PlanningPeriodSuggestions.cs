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
			var now = new DateOnly(_now.UtcDateTime());
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

		public IEnumerable<SuggestedPlanningPeriod> SuggestedPeriods(DateOnlyPeriod range)
		{
			var result = new List<Tuple<int, SuggestedPlanningPeriod>>();
			
			var resultingRanges = _uniqueSchedulePeriods.SelectMany(uniqueSchedulePeriod => 
			{
				var innerResult = new List<Tuple<int,SuggestedPlanningPeriod>>();
				var currentDate = range.StartDate;
				while (currentDate <= range.EndDate)
				{
					var singlePeriod = new Tuple<int,SuggestedPlanningPeriod>(uniqueSchedulePeriod.Priority,new SuggestedPlanningPeriod
					{
						PeriodType = uniqueSchedulePeriod.PeriodType,
						Number = uniqueSchedulePeriod.Number,
						Range =
							_schedulePeriodRangeCalculator.PeriodForType(currentDate,
								new SchedulePeriodForRangeCalculation
								{
									Culture =
										CultureInfo.GetCultureInfo(uniqueSchedulePeriod.Culture.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)),
									Number = uniqueSchedulePeriod.Number,
									PeriodType = uniqueSchedulePeriod.PeriodType,
									StartDate = new DateOnly(uniqueSchedulePeriod.DateFrom)
								})
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
						Range = new DateOnlyPeriod(singlePeriod.Item2.Range.StartDate,
							_schedulePeriodRangeCalculator.PeriodForType(currentDate,
								rangeForDoublePeriod).EndDate)
					});
					innerResult.Add(singlePeriod);
					innerResult.Add(doublePeriod);

				}
				return innerResult;
			});

			result.AddRange(resultingRanges);
			result.Add(new Tuple<int, SuggestedPlanningPeriod>(0,monthIsLastResort(range.StartDate.Date)));
			result.Add(new Tuple<int, SuggestedPlanningPeriod>(0,monthIsLastResort(range.EndDate.Date)));
			return
				result.Where(r => r.Item2.Range.StartDate > new DateOnly(_now.UtcDateTime()))
					.GroupBy(i => i.Item2)
					.Select(s => new {s.Key, Score = s.Sum(v => v.Item1)})
					.OrderByDescending(x => x.Score)
					.ThenBy(y => y.Key.Range.StartDate)
					.Select(z => z.Key)
					.ToArray();
		}

		public IEnumerable<SuggestedPlanningPeriod> NextSuggestedPeriods(DateOnlyPeriod range)
		{
			var result = new List<Tuple<int, SuggestedPlanningPeriod>>();
			var resultingRanges = _uniqueSchedulePeriods.SelectMany(uniqueSchedulePeriod =>
			{
				var innerResult = new List<Tuple<int, SuggestedPlanningPeriod>>();
				var nextDayAfterRange = range.EndDate.AddDays(1);
					var singlePeriod = new Tuple<int, SuggestedPlanningPeriod>(uniqueSchedulePeriod.Priority, new SuggestedPlanningPeriod
					{
						PeriodType = uniqueSchedulePeriod.PeriodType,
						Number = uniqueSchedulePeriod.Number,
						Range =
							_schedulePeriodRangeCalculator.PeriodForType(nextDayAfterRange,
								new SchedulePeriodForRangeCalculation
								{
									Culture =
										CultureInfo.GetCultureInfo(uniqueSchedulePeriod.Culture.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)),
									Number = uniqueSchedulePeriod.Number,
									PeriodType = uniqueSchedulePeriod.PeriodType,
									StartDate = new DateOnly(uniqueSchedulePeriod.DateFrom)
								})
					});

					nextDayAfterRange = singlePeriod.Item2.Range.EndDate.AddDays(1);
					var rangeForDoublePeriod = new SchedulePeriodForRangeCalculation
					{
						Culture =
							CultureInfo.GetCultureInfo(uniqueSchedulePeriod.Culture.GetValueOrDefault(CultureInfo.CurrentCulture.LCID)),
						Number = uniqueSchedulePeriod.Number,
						PeriodType = uniqueSchedulePeriod.PeriodType,
						StartDate = new DateOnly(uniqueSchedulePeriod.DateFrom)
					};
					var doublePeriod = new Tuple<int, SuggestedPlanningPeriod>(uniqueSchedulePeriod.Priority, new SuggestedPlanningPeriod
					{
						PeriodType = uniqueSchedulePeriod.PeriodType,
						Number = uniqueSchedulePeriod.Number * 2,
						Range = new DateOnlyPeriod(singlePeriod.Item2.Range.StartDate,
							_schedulePeriodRangeCalculator.PeriodForType(nextDayAfterRange,
								rangeForDoublePeriod).EndDate)
					});
					innerResult.Add(singlePeriod);
					innerResult.Add(doublePeriod);

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