using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
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
	public class ForecastWorkloadEvaluatorTest
	{
		[Test]
		public void ShouldRemoveOutliersWhenPreforecastButNoComparison()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkillWithId("skill1"));
			workload.SetId(Guid.NewGuid());
			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var dateOnly1 = new DateOnly(2013, 1, 1);
			var workloadDay1 = new WorkloadDay();
			workloadDay1.Create(dateOnly1, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay1 = new ValidatedVolumeDay(workload, dateOnly1)
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(3),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(2),
				TaskOwner = workloadDay1,
				ValidatedTasks = 110
			};

			var dateOnly2 = new DateOnly(2015, 1, 1);
			var workloadDay2 = new WorkloadDay();
			workloadDay2.Create(dateOnly2, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay2 = new ValidatedVolumeDay(workload, dateOnly2)
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(3),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(2),
				TaskOwner = workloadDay2,
				ValidatedTasks = 110
			};

			var taskOwnerPeriod = new TaskOwnerPeriod(new DateOnly(2015,1,1), new ITaskOwner[] { validatedVolumeDay1, validatedVolumeDay2 }, TaskOwnerPeriodType.Other);
			historicalData.Stub(x => x.Fetch(null, new DateOnlyPeriod())).IgnoreArguments().Return(taskOwnerPeriod);

			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			var forecastMethod = MockRepository.GenerateMock<IForecastMethod>();
			forecastMethod.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments().Return(new ForecastResult(){ForecastingTargets = new IForecastingTarget[]{}});
			forecastMethodProvider.Stub(x => x.All()).Return(new[] {forecastMethod,forecastMethod});
			var target = new ForecastWorkloadEvaluator(historicalData,
				MockRepository.GenerateMock<IForecastAccuracyCalculator>(), forecastMethodProvider, 
				MockRepository.GenerateMock<IHistoricalPeriodProvider>(), outlierRemover);
			target.Evaluate(workload);

			outlierRemover.AssertWasCalled(x => x.RemoveOutliers(Arg<ITaskOwnerPeriod>.Is.Anything, Arg<IForecastMethod>.Is.Anything), options => options.Repeat.Twice());
		}
	}
}