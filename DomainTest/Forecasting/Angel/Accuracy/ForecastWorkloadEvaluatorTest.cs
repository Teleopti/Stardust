using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
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
			var historicalData = new FakeHistoricalData();

			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			var forecastMethod = MockRepository.GenerateMock<IForecastMethod>();
			forecastMethod.Stub(x => x.ForecastTasks(null, new DateOnlyPeriod())).IgnoreArguments().Return(new Dictionary<DateOnly, double>());
			forecastMethodProvider.Stub(x => x.Calculate(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { forecastMethod, forecastMethod });
			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload)).Return(new DateOnlyPeriod(2013, 1, 1, 2015, 1, 1));
			var forecastAccuracyCalculator = MockRepository.GenerateMock<IForecastAccuracyCalculator>();
			forecastAccuracyCalculator.Stub(x => x.Accuracy(null, null, null, null)).IgnoreArguments()
				.Return(new AccuracyModel { TasksPercentageError = 1d, TaskTimePercentageError = 1d });
			var target = new ForecastWorkloadEvaluator(historicalData, forecastMethodProvider, 
				historicalPeriodProvider);
			target.Evaluate(workload, outlierRemover, forecastAccuracyCalculator);

			outlierRemover.AssertWasCalled(x => x.RemoveOutliers(Arg<ITaskOwnerPeriod>.Is.Anything,Arg<IForecastMethod>.Is.Anything,
					Arg<IForecastMethod>.Is.Anything, Arg<IForecastMethod>.Is.Anything), options => options.Repeat.Twice());
		}

		[Test]
		public void ShouldLoadHistoricalDataOnlyOnceWithMultipleForecastMethod()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkillWithId("skill1"));
			workload.SetId(Guid.NewGuid());
			var historicalData = new FakeHistoricalDataWithCounter();

			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			var forecastMethod = MockRepository.GenerateMock<IForecastMethod>();
			forecastMethod.Stub(x => x.ForecastTasks(null, new DateOnlyPeriod())).IgnoreArguments()
				.Return(new Dictionary<DateOnly, double>());
			forecastMethodProvider.Stub(x => x.Calculate(Arg<DateOnlyPeriod>.Is.Anything))
				.Return(new[] {forecastMethod, forecastMethod});
			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload))
				.Return(new DateOnlyPeriod(2013, 1, 1, 2015, 1, 1));
			var target =
				new ForecastWorkloadEvaluator(historicalData, forecastMethodProvider, historicalPeriodProvider);
			target.Evaluate(workload, outlierRemover, MockRepository.GenerateMock<IForecastAccuracyCalculator>());

			historicalData.FetchCount.Should().Be.EqualTo(1);
		}
	}
}