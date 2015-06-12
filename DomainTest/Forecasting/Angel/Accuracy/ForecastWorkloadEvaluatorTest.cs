using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
	public class FakeHistoricalData : IHistoricalData
	{

		public TaskOwnerPeriod Fetch(IWorkload workload, DateOnlyPeriod period)
		{
			var dateOnly1 = period.StartDate;
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(dateOnly1, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay1 = new ValidatedVolumeDay(workload, dateOnly1)
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(3),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(2),
				TaskOwner = workloadDay1,
				ValidatedTasks = 110
			};

			var dateOnly2 = period.EndDate;
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(dateOnly2, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay2 = new ValidatedVolumeDay(workload, dateOnly2)
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(3),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(2),
				TaskOwner = workloadDay2,
				ValidatedTasks = 110
			};
			var taskOwnerDays = new ITaskOwner[] { validatedVolumeDay1, validatedVolumeDay2 };
			var taskOwnerPeriod = new TaskOwnerPeriod(new DateOnly(), taskOwnerDays, TaskOwnerPeriodType.Other);
			return taskOwnerPeriod;
		}
	}

	public class ForecastWorkloadEvaluatorTest
	{
		[Test]
		public void ShouldReturnPeriodEvaluateOn()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkillWithId("skill1"));
			workload.SetId(Guid.NewGuid());
			var historicalData = new FakeHistoricalData();

			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			var forecastMethod = MockRepository.GenerateMock<IForecastMethod>();
			forecastMethod.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments().Return(new ForecastResult { ForecastingTargets = new IForecastingTarget[] { } });
			forecastMethodProvider.Stub(x => x.All()).Return(new[] { forecastMethod });

			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.PeriodForForecast(workload)).Return(new DateOnlyPeriod(2012, 3, 16, 2015, 3, 15));
			var target = new ForecastWorkloadEvaluator(historicalData,
				MockRepository.GenerateMock<IForecastAccuracyCalculator>(), forecastMethodProvider,
				historicalPeriodProvider, outlierRemover);
			var result = target.Evaluate(workload);
			result.Accuracies.First().PeriodEvaluateOn.StartDate.Should().Be.EqualTo(new DateOnly(2014, 3, 16));
			result.Accuracies.First().PeriodEvaluateOn.EndDate.Should().Be.EqualTo(new DateOnly(2015, 3, 15));
		}

		[Test]
		public void ShouldReturnPeriodUsedToEvaluate()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkillWithId("skill1"));
			workload.SetId(Guid.NewGuid());
			var historicalData = new FakeHistoricalData();

			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			var forecastMethod = MockRepository.GenerateMock<IForecastMethod>();
			forecastMethod.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments().Return(new ForecastResult { ForecastingTargets = new IForecastingTarget[] { } });
			forecastMethodProvider.Stub(x => x.All()).Return(new[] { forecastMethod });

			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.PeriodForForecast(workload)).Return(new DateOnlyPeriod(2012, 3, 16, 2015, 3, 15));
			var target = new ForecastWorkloadEvaluator(historicalData,
				MockRepository.GenerateMock<IForecastAccuracyCalculator>(), forecastMethodProvider,
				historicalPeriodProvider, outlierRemover);
			var result = target.Evaluate(workload);
			result.Accuracies.First().PeriodUsedToEvaluate.StartDate.Should().Be.EqualTo(new DateOnly(2012, 3, 16));
			result.Accuracies.First().PeriodUsedToEvaluate.EndDate.Should().Be.EqualTo(new DateOnly(2014, 3, 15));
		}

		[Test]
		public void ShouldRemoveOutliersWhenPreforecastButNoComparison()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkillWithId("skill1"));
			workload.SetId(Guid.NewGuid());
			var historicalData = new FakeHistoricalData();

			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			var forecastMethod = MockRepository.GenerateMock<IForecastMethod>();
			forecastMethod.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments().Return(new ForecastResult {ForecastingTargets = new IForecastingTarget[]{}});
			forecastMethodProvider.Stub(x => x.All()).Return(new[] {forecastMethod,forecastMethod});
			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.PeriodForForecast(workload)).Return(new DateOnlyPeriod(2013, 1, 1, 2015, 1, 1));
			var target = new ForecastWorkloadEvaluator(historicalData,
				MockRepository.GenerateMock<IForecastAccuracyCalculator>(), forecastMethodProvider, 
				historicalPeriodProvider, outlierRemover);
			target.Evaluate(workload);

			outlierRemover.AssertWasCalled(x => x.RemoveOutliers(Arg<ITaskOwnerPeriod>.Is.Anything, Arg<IForecastMethod>.Is.Anything), options => options.Repeat.Twice());
		}
	}
}