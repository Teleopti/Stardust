using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class OnlyValidatedTest : QuickForecastTest
	{
		private const int expectedNumberOfTasks = 35;

		protected override IEnumerable<DailyStatistic> DailyStatistics()
		{
			return Enumerable.Empty<DailyStatistic>();
		}

		protected override IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			var historialWorkloadDay = new WorkloadDay();
			historialWorkloadDay.Create(HistoricalPeriod.StartDate, Workload, new TimePeriod[] { });
			return new[]
			{
				new ValidatedVolumeDay(Workload, HistoricalPeriod.StartDate)
				{
					TaskOwner = historialWorkloadDay,
					ValidatedTasks = expectedNumberOfTasks
				}
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			Convert.ToInt32(modifiedSkillDays.Single().Tasks)
				.Should().Be.EqualTo(expectedNumberOfTasks);
		}
	}
}