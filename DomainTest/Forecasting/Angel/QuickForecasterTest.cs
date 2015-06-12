using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Infrastructure.Forecasting.Angel;
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
			var historicalPeriodForForecast1 = new DateOnlyPeriod(2014, 1, 1, 2014, 12, 31);
			var historicalPeriodForForecast2 = new DateOnlyPeriod(2013, 12, 31, 2014, 12, 30);
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
			var quickForecastWorkloadEvaluator = MockRepository.GenerateMock<IForecastWorkloadEvaluator>();
			quickForecastWorkloadEvaluator.Stub(x => x.Evaluate(workload1))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy{MethodId = ForecastMethodType.TeleoptiClassic},
							new MethodAccuracy{MethodId = ForecastMethodType.TeleoptiClassicWithTrend, IsSelected = true}
						}
				});

			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload1)).Return(historicalPeriodForForecast1);
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload2)).Return(historicalPeriodForForecast2);
			var target = new QuickForecaster(quickForecasterWorkload, fetchAndFillSkillDays, quickForecastWorkloadEvaluator, historicalPeriodProvider);
			target.ForecastWorkloadsWithinSkill(skill1, new[] { new ForecastWorkloadInput { WorkloadId = workload1.Id.Value, ForecastMethodId = ForecastMethodType.None } }, futurePeriod);
			quickForecasterWorkload.AssertWasCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriodForForecast1,
				SkillDays = skillDays,
				WorkLoad = workload1,
				ForecastMethodId = ForecastMethodType.TeleoptiClassicWithTrend
			}));

			quickForecasterWorkload.AssertWasNotCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriodForForecast2,
				SkillDays = skillDays,
				WorkLoad = workload2,
				ForecastMethodId = ForecastMethodType.TeleoptiClassicWithTrend
			}));
		}

		[Test]
		public void ShouldUseSelectedForecastMethod()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var futurePeriod = new DateOnlyPeriod(2015, 1, 2, 2015, 2, 2);
			var historicalPeriodForForecast = new DateOnlyPeriod(2014, 1, 1, 2014, 12, 31);
			var fetchAndFillSkillDays = MockRepository.GenerateMock<IFetchAndFillSkillDays>();
			var skillDay = SkillDayFactory.CreateSkillDay(skill1, new DateOnly(2015, 1, 3));
			skillDay.Skill.WorkloadCollection.ForEach(w => w.SetId(Guid.NewGuid()));
			var skillDays = new[] { skillDay };
			fetchAndFillSkillDays.Stub(x => x.FindRange(futurePeriod, skill1)).Return(skillDays);
			var quickForecasterWorkload = MockRepository.GenerateMock<IQuickForecasterWorkload>();
			var workload1 = WorkloadFactory.CreateWorkload("workload1", skill1);
			workload1.SetId(Guid.NewGuid());
			var quickForecastWorkloadEvaluator = MockRepository.GenerateMock<IForecastWorkloadEvaluator>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload1)).Return(historicalPeriodForForecast);
			var target = new QuickForecaster(quickForecasterWorkload, fetchAndFillSkillDays, quickForecastWorkloadEvaluator, historicalPeriodProvider);
			target.ForecastWorkloadsWithinSkill(skill1, new[] { new ForecastWorkloadInput { WorkloadId = workload1.Id.Value, ForecastMethodId = ForecastMethodType.TeleoptiClassic} }, futurePeriod);
			quickForecastWorkloadEvaluator.AssertWasNotCalled(x => x.Evaluate(workload1));
			quickForecasterWorkload.AssertWasCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriodForForecast,
				SkillDays = skillDays,
				WorkLoad = workload1,
				ForecastMethodId = ForecastMethodType.TeleoptiClassic
			}));
		}

		[Test]
		public void ShouldPickBestForecastMethodWhenNoMethodSelected()
		{
			var skill1 = SkillFactory.CreateSkill("skill1");
			var futurePeriod = new DateOnlyPeriod(2015, 1, 2, 2015, 2, 2);
			var historicalPeriodForForecast = new DateOnlyPeriod(2014, 1, 1, 2014, 12, 31);
			var fetchAndFillSkillDays = MockRepository.GenerateMock<IFetchAndFillSkillDays>();
			var skillDay = SkillDayFactory.CreateSkillDay(skill1, new DateOnly(2015, 1, 3));
			skillDay.Skill.WorkloadCollection.ForEach(w => w.SetId(Guid.NewGuid()));
			var skillDays = new[] {skillDay};
			fetchAndFillSkillDays.Stub(x => x.FindRange(futurePeriod, skill1)).Return(skillDays);
			var quickForecasterWorkload = MockRepository.GenerateMock<IQuickForecasterWorkload>();
			var workload1 = WorkloadFactory.CreateWorkload("workload1", skill1);
			workload1.SetId(Guid.NewGuid());
			var quickForecastWorkloadEvaluator = MockRepository.GenerateMock<IForecastWorkloadEvaluator>();
			quickForecastWorkloadEvaluator.Stub(x => x.Evaluate(workload1))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy{MethodId = ForecastMethodType.TeleoptiClassic},
							new MethodAccuracy{MethodId = ForecastMethodType.TeleoptiClassicWithTrend, IsSelected = true}
						}
				});
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload1)).Return(historicalPeriodForForecast);
			var target = new QuickForecaster(quickForecasterWorkload, fetchAndFillSkillDays, quickForecastWorkloadEvaluator, historicalPeriodProvider);
			target.ForecastWorkloadsWithinSkill(skill1,new[] {new ForecastWorkloadInput {WorkloadId = workload1.Id.Value, ForecastMethodId = ForecastMethodType.None}}, futurePeriod);
			quickForecasterWorkload.AssertWasCalled(x => x.Execute(new QuickForecasterWorkloadParams
			{
				FuturePeriod = futurePeriod,
				HistoricalPeriod = historicalPeriodForForecast,
				SkillDays = skillDays,
				WorkLoad = workload1,
				ForecastMethodId = ForecastMethodType.TeleoptiClassicWithTrend
			}));
		}
	}
}