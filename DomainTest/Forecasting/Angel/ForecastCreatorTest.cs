using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class ForecastCreatorTest
	{
		[Test]
		public void ShouldForecastForWorkloads()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] { skill1, skill2 });
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var workload1 = WorkloadFactory.CreateWorkload("workload1", skill1);
			workload1.SetId(id1);
			var workload2 = WorkloadFactory.CreateWorkload("workload2", skill2);
			workload2.SetId(id2);
			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var futurePeriod = new DateOnlyPeriod();

			var target = new ForecastCreator(quickForecaster, skillRepository);
			var forecastWorkloadInputs = new[]
			{
				new ForecastWorkloadInput
				{
					WorkloadId = id1,
					ForecastMethodId = ForecastMethodType.TeleoptiClassicLongTerm
				},
				new ForecastWorkloadInput
				{
					WorkloadId = id2,
					ForecastMethodId = ForecastMethodType.TeleoptiClassicLongTerm
				}
			};
			var scenario = new Scenario("test1");
			target.CreateForecastForWorkloads(futurePeriod, forecastWorkloadInputs, scenario);
			quickForecaster.AssertWasCalled(x => x.ForecastWorkloadsWithinSkill(skill1, forecastWorkloadInputs, futurePeriod, scenario));
			quickForecaster.AssertWasCalled(x => x.ForecastWorkloadsWithinSkill(skill2, forecastWorkloadInputs, futurePeriod, scenario));
		}
	}
}