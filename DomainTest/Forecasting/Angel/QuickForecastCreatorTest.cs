using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecastCreatorTest
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
			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriodForForecast = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);
			var historicalPeriodForMeasurement = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);

			var target = new QuickForecastCreator(quickForecaster, skillRepository, now);
			var forecastWorkloadInputs = new[]
			{
				new ForecastWorkloadInput
				{
					WorkloadId = id1,
					ForecastMethodId = ForecastMethodType.TeleoptiClassic
				},
				new ForecastWorkloadInput
				{
					WorkloadId = id2,
					ForecastMethodId = ForecastMethodType.TeleoptiClassic
				}
			};
			target.CreateForecastForWorkloads(futurePeriod, forecastWorkloadInputs);
			quickForecaster.AssertWasCalled(x => x.ForecastWorkloadsWithinSkill(skill1, forecastWorkloadInputs, futurePeriod, historicalPeriodForForecast, historicalPeriodForMeasurement));
			quickForecaster.AssertWasCalled(x => x.ForecastWorkloadsWithinSkill(skill2, forecastWorkloadInputs, futurePeriod, historicalPeriodForForecast, historicalPeriodForMeasurement));
		}

		[Test]
		public void ShouldForecastForAll()
		{
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkill("skill1");
			var skill2 = SkillFactory.CreateSkill("skill2");
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] { skill1, skill2 });

			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var futurePeriod = new DateOnlyPeriod();
			var now = new Now();
			var nowDate = now.LocalDateOnly();
			var historicalPeriodForForecast = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);
			var historicalPeriodForMeasurement = new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);

			var target = new QuickForecastCreator(quickForecaster, skillRepository, now);
			target.CreateForecastForAll(futurePeriod);
			quickForecaster.AssertWasCalled(x => x.ForecastAll(skill1, futurePeriod, historicalPeriodForForecast, historicalPeriodForMeasurement));
			quickForecaster.AssertWasCalled(x => x.ForecastAll(skill2, futurePeriod, historicalPeriodForForecast, historicalPeriodForMeasurement));
		}
	}
}