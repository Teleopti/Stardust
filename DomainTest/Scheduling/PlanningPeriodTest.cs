using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class PlanningPeriodTest
	{
		[Test, SetCulture("sv-SE")]
		public void ShouldGetUpcommingMonthAsDefaultPlanningPeriod()
		{
			var target = new PlanningPeriod( new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015,4,1)),new List<AggregatedSchedulePeriod>()), new PlanningGroup());
			
			target.Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnNextPlanningPeriod()
		{
			var target = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()), new PlanningGroup());
			target.NextPlanningPeriod(null).Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 06, 01, 2015, 06, 30));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnNextPlanningPeriodForPlanningGroup()
		{
			var target = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()), new PlanningGroup());
			var planningGroup = new PlanningGroup("group1");
			var nextPlanningPeriod = target.NextPlanningPeriod(planningGroup);
			nextPlanningPeriod.Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 06, 01, 2015, 06, 30));
			nextPlanningPeriod.PlanningGroup.Name.Should().Be.EqualTo(planningGroup.Name);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnNextNextPlanningPeriod()
		{
			var target = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()), new PlanningGroup());
			target.NextPlanningPeriod(null).NextPlanningPeriod(null).Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 07, 01, 2015, 07, 31));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnPlanningPeriodWithDefaultRange()
		{
			var culture =
				CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.LCID);
			var aggSchedulePeriod1 = new AggregatedSchedulePeriod
			{
				Number = 2,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 10
			};
			var aggSchedulePeriod2 = new AggregatedSchedulePeriod
			{
				Number = 1,
				Culture = 1053,
				DateFrom = new DateTime(2015, 05, 04),
				PeriodType = SchedulePeriodType.Week,
				Priority = 5
			};
			var suggestion = new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 05, 23)), new List<AggregatedSchedulePeriod>
			{
				aggSchedulePeriod1,aggSchedulePeriod2
			});

			var target = new PlanningPeriod(suggestion, new PlanningGroup());
			target.ChangeRange(new SchedulePeriodForRangeCalculation { Culture = culture, Number = 1, PeriodType = SchedulePeriodType.Week, StartDate = new DateOnly(2015, 06, 01) });
			target.NextPlanningPeriod(null).Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 06, 8, 2015, 06, 21));
		}

		[Test]
		public void ShouldChangeTypeOfPeriodAndNextPeriodShouldUseSameWeek()
		{
			var target = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 05, 23)), new List<AggregatedSchedulePeriod>()), new PlanningGroup());

			var startDate = new DateOnly(2017, 04, 03);
			target.ChangeRange(new SchedulePeriodForRangeCalculation
			{
				Number = 14,
				PeriodType = SchedulePeriodType.Day,
				StartDate = startDate
			}, true);

			target.Range.StartDate.Should().Be.EqualTo(startDate);
			target.Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 04, 16));

			var nextPeriod = target.NextPlanningPeriod(null);
			nextPeriod.Range.StartDate.Should().Be.EqualTo(new DateOnly(2017, 04, 17));
			nextPeriod.Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 04, 30));
		}

		[Test]
		public void ShouldChangeTypeOfPeriodAndNextPeriodShouldUseSameMonth()
		{
			var target = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 05, 23)), new List<AggregatedSchedulePeriod>()), new PlanningGroup());

			var startDate = new DateOnly(2017, 04, 01);
			target.ChangeRange(new SchedulePeriodForRangeCalculation
			{
				Number = 1,
				PeriodType = SchedulePeriodType.Month,
				StartDate = startDate
			}, true);

			target.Range.StartDate.Should().Be.EqualTo(startDate);
			target.Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 04, 30));

			var nextPeriod = target.NextPlanningPeriod(null);
			nextPeriod.Range.StartDate.Should().Be.EqualTo(new DateOnly(2017, 05, 01));
			nextPeriod.Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 05, 31));
		}

		[Test]
		public void ShouldChangeTypeOfPeriodAndNextPeriodShouldUseSameDays()
		{
			var target = new PlanningPeriod(new PlanningPeriodSuggestions(new MutableNow(new DateTime(2015, 05, 23)), new List<AggregatedSchedulePeriod>()), new PlanningGroup());

			var startDate = new DateOnly(2017, 04, 01);
			target.ChangeRange(new SchedulePeriodForRangeCalculation
			{
				Number = 8,
				PeriodType = SchedulePeriodType.Day,
				StartDate = startDate
			}, true);

			target.Range.StartDate.Should().Be.EqualTo(startDate);
			target.Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 04, 08));

			var nextPeriod = target.NextPlanningPeriod(null);
			nextPeriod.Range.StartDate.Should().Be.EqualTo(new DateOnly(2017, 04, 09));
			nextPeriod.Range.EndDate.Should().Be.EqualTo(new DateOnly(2017, 04, 16));
		}

		[Test]
		public void ShouldReturnMonthPeriodEvenIfStartDateIsInMiddleOfMonth()
		{
			var target = new PlanningPeriod(new DateOnlyPeriod(2017, 12, 10, 2018, 1, 9),SchedulePeriodType.Month,1, new PlanningGroup());
			target.PeriodType.Should().Be.EqualTo(SchedulePeriodType.Month);
			target.Number.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnMonthPeriodWhenStartDateIs31()
		{
			var target = new PlanningPeriod(new DateOnlyPeriod(2017, 3, 31, 2017, 4, 29),SchedulePeriodType.Month,1, new PlanningGroup());
			target.PeriodType.Should().Be.EqualTo(SchedulePeriodType.Month);
			target.Number.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnMonthPeriodWhenItIsOneMonthLongAndEndDateIsFebruary27()
		{
			var target = new PlanningPeriod(new DateOnlyPeriod(2017, 1, 30, 2017, 2, 27),SchedulePeriodType.Month, 1, new PlanningGroup());
			target.PeriodType.Should().Be.EqualTo(SchedulePeriodType.Month);
			target.Number.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnMonthPeriodWhenItIsTwoMonthsLongAndEndDateIsFebruary27()
		{
			var target = new PlanningPeriod(new DateOnlyPeriod(2016, 12, 29, 2017, 2, 27),SchedulePeriodType.Month,2, new PlanningGroup());
			target.PeriodType.Should().Be.EqualTo(SchedulePeriodType.Month);
			target.Number.Should().Be.EqualTo(2);
		}
	}
}
