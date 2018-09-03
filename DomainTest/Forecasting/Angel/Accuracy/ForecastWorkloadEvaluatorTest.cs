using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Accuracy
{
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
			forecastMethod.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments().Return(new IForecastingTarget[] {});
			forecastMethodProvider.Stub(x => x.Calculate(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { forecastMethod });

			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload)).Return(new DateOnlyPeriod(2012, 3, 16, 2015, 3, 15));
			var target =
				new ForecastWorkloadEvaluator(historicalData, forecastMethodProvider, historicalPeriodProvider);
			var result = target.Evaluate(workload, outlierRemover, MockRepository.GenerateMock<IForecastAccuracyCalculator>());
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
			forecastMethod.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments().Return(new IForecastingTarget[] {});
			forecastMethodProvider.Stub(x => x.Calculate(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { forecastMethod });

			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload)).Return(new DateOnlyPeriod(2012, 3, 16, 2015, 3, 15));
			var target = new ForecastWorkloadEvaluator(historicalData, forecastMethodProvider,
				historicalPeriodProvider);
			var result = target.Evaluate(workload, outlierRemover, MockRepository.GenerateMock<IForecastAccuracyCalculator>());
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
			forecastMethod.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments().Return(new IForecastingTarget[] {});
			forecastMethodProvider.Stub(x => x.Calculate(Arg<DateOnlyPeriod>.Is.Anything)).Return(new[] { forecastMethod, forecastMethod });
			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			historicalPeriodProvider.Stub(x => x.AvailablePeriod(workload)).Return(new DateOnlyPeriod(2013, 1, 1, 2015, 1, 1));
			var target = new ForecastWorkloadEvaluator(historicalData, forecastMethodProvider, 
				historicalPeriodProvider);
			target.Evaluate(workload, outlierRemover, MockRepository.GenerateMock<IForecastAccuracyCalculator>());

			outlierRemover.AssertWasCalled(x => x.RemoveOutliers(Arg<ITaskOwnerPeriod>.Is.Anything,
				Arg<IForecastMethod>.Is.Anything), options => options.Repeat.Twice());
		}

		[Test]
		public void ShouldLoadHistoricalDataOnlyOnceWithMultipleForecastMethod()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkillWithId("skill1"));
			workload.SetId(Guid.NewGuid());
			var historicalData = new FakeHistoricalDataWithCounter();

			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			var forecastMethod = MockRepository.GenerateMock<IForecastMethod>();
			forecastMethod.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments()
				.Return(new IForecastingTarget[] { });
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