using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	public class PlanningPeriodTest
	{
		[Test, SetCulture("sv-SE")]
		public void ShouldGetUpcommingMonthAsDefaultPlanningPeriod()
		{
			var target = new PlanningPeriod( new PlanningPeriodSuggestions(new TestableNow(new DateTime(2015,4,1)),new List<AggregatedSchedulePeriod>()));
			
			target.Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 05, 01, 2015, 05, 31));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnNextPlanningPeriod()
		{
			var target = new PlanningPeriod(new PlanningPeriodSuggestions(new TestableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			target.NextPlanningPeriod().Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 06, 01, 2015, 06, 30));
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldReturnNextNextPlanningPeriod()
		{
			var target = new PlanningPeriod(new PlanningPeriodSuggestions(new TestableNow(new DateTime(2015, 4, 1)), new List<AggregatedSchedulePeriod>()));
			target.NextPlanningPeriod().NextPlanningPeriod().Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 07, 01, 2015, 07, 31));
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
			var suggestion = new PlanningPeriodSuggestions(new TestableNow(new DateTime(2015, 05, 23)), new List<AggregatedSchedulePeriod>
			{
				aggSchedulePeriod1,aggSchedulePeriod2
			});

			var target = new PlanningPeriod(suggestion);
			target.ChangeRange(new SchedulePeriodForRangeCalculation { Culture = culture, Number = 1, PeriodType = SchedulePeriodType.Week, StartDate = new DateOnly(2015, 06, 01) });
			target.NextPlanningPeriod().Range.Should().Be.EqualTo(new DateOnlyPeriod(2015, 06, 8, 2015, 06, 21));
		}
	}
}
