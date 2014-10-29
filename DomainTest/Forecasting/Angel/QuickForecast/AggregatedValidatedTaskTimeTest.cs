using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecast
{
	public class AggregatedValidatedTaskTimeTest : QuickForecastWorkloadTest
	{
		private readonly TimeSpan expectedTaskTime = TimeSpan.FromSeconds(12);
		private readonly TimeSpan expectedAfterTaskTime = TimeSpan.FromSeconds(122);

		protected override IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays()
		{
			var historialWorkloadDay = new WorkloadDay();
			historialWorkloadDay.Create(HistoricalPeriod.StartDate, Workload, new TimePeriod[] {});
			yield return new ValidatedVolumeDay(Workload, HistoricalPeriod.StartDate)
			{
				TaskOwner = historialWorkloadDay,
				ValidatedAverageTaskTime = expectedTaskTime,
				ValidatedAverageAfterTaskTime = expectedAfterTaskTime
			};
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			var skillDay = modifiedSkillDays.Single();
			skillDay.AverageTaskTime
				.Should().Be.EqualTo(expectedTaskTime);
			skillDay.AverageAfterTaskTime
				.Should().Be.EqualTo(expectedAfterTaskTime);
		}
	}
}