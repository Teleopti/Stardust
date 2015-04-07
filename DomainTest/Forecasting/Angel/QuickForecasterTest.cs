using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecasterTest
	{
		[Test]
		public void ShouldForecastWorkloadsInSameSkill()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var futurePeriod = new DateOnlyPeriod(2015, 1, 1, 2015, 2, 1);
			var historicalPeriodForForecast = new DateOnlyPeriod(2014, 1, 1, 2014, 12, 31);
			var historicalPeriodForMeasurement = new DateOnlyPeriod(2013, 1, 1, 2015, 12, 31);
			var fetchAndFillSkillDays = MockRepository.GenerateMock<IFetchAndFillSkillDays>();
			var skillDay = SkillDayFactory.CreateSkillDay(skill1, new DateOnly(2015, 1, 3));
			skillDay.Skill.WorkloadCollection.ForEach(w => w.SetId(Guid.NewGuid()));
			var skillDays = new[] {skillDay};
			fetchAndFillSkillDays.Stub(x => x.FindRange(futurePeriod, skill1)).Return(skillDays);
			var quickForecasterWorkload = MockRepository.GenerateMock<IQuickForecasterWorkload>();
			var workload1 = WorkloadFactory.CreateWorkload("workload1", skill1);
			workload1.SetId(Guid.NewGuid());
			var workload2 = WorkloadFactory.CreateWorkload("workload2", skill1);
			workload2.SetId(Guid.NewGuid());
			var quickForecastWorkloadEvaluator = MockRepository.GenerateMock<IQuickForecastWorkloadEvaluator>();
			quickForecastWorkloadEvaluator.Stub(x => x.Measure(workload1, historicalPeriodForMeasurement))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy{MethodId = ForecastMethodType.TeleoptiClassic},
							new MethodAccuracy{MethodId = ForecastMethodType.TeleoptiClassicWithTrend, IsSelected = true}
						}
				});

			var target = new QuickForecaster(quickForecasterWorkload, fetchAndFillSkillDays, quickForecastWorkloadEvaluator);
			target.ForecastWorkloadsWithinSkill(skill1, new[] { new ForecastWorkloadInput { WorkloadId = workload1.Id.Value, ForecastMethodId = ForecastMethodType.TeleoptiClassic } }, futurePeriod, historicalPeriodForForecast, historicalPeriodForMeasurement);
			quickForecasterWorkload.AssertWasCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriodForForecast,
				SkillDays = skillDays,
				WorkLoad = workload1,
				ForecastMethodId = ForecastMethodType.TeleoptiClassicWithTrend
			}));

			quickForecasterWorkload.AssertWasNotCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriodForForecast,
				SkillDays = skillDays,
				WorkLoad = workload2,
				ForecastMethodId = ForecastMethodType.TeleoptiClassicWithTrend
			}));
		}

		[Test]
		public void ShouldFillForecastMethodType()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var futurePeriod = new DateOnlyPeriod(2015, 1, 2, 2015, 2, 2);
			var historicalPeriodForForecast = new DateOnlyPeriod(2014, 1, 1, 2014, 12, 31);
			var historicalPeriodForMeasurement = new DateOnlyPeriod(2013, 1, 1, 2014, 12, 31);
			var fetchAndFillSkillDays = MockRepository.GenerateMock<IFetchAndFillSkillDays>();
			var skillDay = SkillDayFactory.CreateSkillDay(skill1, new DateOnly(2015, 1, 3));
			skillDay.Skill.WorkloadCollection.ForEach(w => w.SetId(Guid.NewGuid()));
			var skillDays = new[] {skillDay};
			fetchAndFillSkillDays.Stub(x => x.FindRange(futurePeriod, skill1)).Return(skillDays);
			var quickForecasterWorkload = MockRepository.GenerateMock<IQuickForecasterWorkload>();
			var workload1 = WorkloadFactory.CreateWorkload("workload1", skill1);
			workload1.SetId(Guid.NewGuid());
			var quickForecastWorkloadEvaluator = MockRepository.GenerateMock<IQuickForecastWorkloadEvaluator>();
			quickForecastWorkloadEvaluator.Stub(x => x.Measure(workload1, historicalPeriodForMeasurement))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy{MethodId = ForecastMethodType.TeleoptiClassic},
							new MethodAccuracy{MethodId = ForecastMethodType.TeleoptiClassicWithTrend, IsSelected = true}
						}
				});
			var target = new QuickForecaster(quickForecasterWorkload, fetchAndFillSkillDays, quickForecastWorkloadEvaluator);
			target.ForecastWorkloadsWithinSkill(skill1,new[] {new ForecastWorkloadInput {WorkloadId = workload1.Id.Value}}, futurePeriod, historicalPeriodForForecast, historicalPeriodForMeasurement);
			quickForecasterWorkload.AssertWasCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriodForForecast,
				SkillDays = skillDays,
				WorkLoad = workload1,
				ForecastMethodId = ForecastMethodType.TeleoptiClassicWithTrend
			}));
		}

		[Test]
		public void ShouldForecastAll()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var futurePeriod = new DateOnlyPeriod(2015, 1, 1, 2015, 2, 1);
			var historicalPeriod = new DateOnlyPeriod(2014, 1, 1, 2014, 12, 31);
			var fetchAndFillSkillDays = MockRepository.GenerateMock<IFetchAndFillSkillDays>();
			var skillDay = SkillDayFactory.CreateSkillDay(skill1, new DateOnly(2015, 1, 3));
			skillDay.Skill.WorkloadCollection.ForEach(w => w.SetId(Guid.NewGuid()));
			var skillDays = new[] { skillDay };
			fetchAndFillSkillDays.Stub(x => x.FindRange(futurePeriod, skill1)).Return(skillDays);
			var quickForecasterWorkload = MockRepository.GenerateMock<IQuickForecasterWorkload>();
			var workload1 = WorkloadFactory.CreateWorkload("workload1", skill1);
			workload1.SetId(Guid.NewGuid());
			var workload2 = WorkloadFactory.CreateWorkload("workload2", skill1);
			workload2.SetId(Guid.NewGuid());

			var target = new QuickForecaster(quickForecasterWorkload, fetchAndFillSkillDays, null);
			target.ForecastAll(skill1, futurePeriod, historicalPeriod);
			quickForecasterWorkload.AssertWasCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriod,
				SkillDays = skillDays,
				WorkLoad = workload1
			}));

			quickForecasterWorkload.AssertWasCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriod,
				SkillDays = skillDays,
				WorkLoad = workload2
			}));
		}
	}
}