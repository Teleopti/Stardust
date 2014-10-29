using System.Collections.Generic;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.QuickForecast
{
	public class NeitherStatisticsNorValidatedTest : QuickForecastWorkloadTest
	{
		protected override IEnumerable<StatisticTask> StatisticTasks()
		{
			yield break;
		}

		protected override void Assert(IEnumerable<ISkillDay> modifiedSkillDays)
		{
			modifiedSkillDays.Should().Be.Empty();
		}
	}
}