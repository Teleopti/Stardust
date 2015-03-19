using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecasterTest
	{
		[Test]
		public void ShouldForecastForWorkload()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var futurePeriod = new DateOnlyPeriod(2015, 1, 1, 2015, 2, 1);
			var historicalPeriod = new DateOnlyPeriod(2014, 1, 1, 2014, 12, 31);
			var fetchAndFillSkillDays = MockRepository.GenerateMock<IFetchAndFillSkillDays>();
			var skillDays = new[] {SkillDayFactory.CreateSkillDay(skill1, new DateTime(2015, 1, 3))};
			fetchAndFillSkillDays.Stub(x => x.FindRange(futurePeriod, skill1)).Return(skillDays);
			var quickForecasterWorkload = MockRepository.GenerateMock<IQuickForecasterWorkload>();
			var workload = WorkloadFactory.CreateWorkload("workload1", skill1);
			
			var target = new QuickForecaster(quickForecasterWorkload, fetchAndFillSkillDays);
			target.ForecastForWorkload(workload, futurePeriod, historicalPeriod);
			quickForecasterWorkload.AssertWasCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriod,
				SkillDays = skillDays,
				WorkLoad = workload
			}));
		}
	}
}