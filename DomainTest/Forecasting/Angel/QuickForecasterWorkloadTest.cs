using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecasterWorkloadTest
	{
		[Test]
		public void ShouldRemoveOutliers()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkillWithId("skill1"));
			var quickForecasterWorkloadParams = new QuickForecasterWorkloadParams()
			{
				WorkLoad = workload,
				HistoricalPeriod = new DateOnlyPeriod(2015,1,1,2015,2,1),
				ForecastMethodId = ForecastMethodType.TeleoptiClassicLongTerm
			};
			var outlierRemover = MockRepository.GenerateMock<IOutlierRemover>();
			var forecastMethodProvider = MockRepository.GenerateMock<IForecastMethodProvider>();
			var method = MockRepository.GenerateMock<IForecastMethod>();
			method.Stub(x => x.Forecast(null, new DateOnlyPeriod())).IgnoreArguments().Return(new ForecastResult());
			forecastMethodProvider.Stub(x => x.Get(quickForecasterWorkloadParams.ForecastMethodId)).Return(method);
			var historicalData = MockRepository.GenerateMock<IHistoricalData>();
			var dateOnly = new DateOnly(2015, 1, 1);

			var workloadDay = new WorkloadDay();
			workloadDay.Create(dateOnly, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });

			var validatedVolumeDay = new ValidatedVolumeDay(workload, dateOnly)
			{
				ValidatedAverageAfterTaskTime = TimeSpan.FromSeconds(3),
				ValidatedAverageTaskTime = TimeSpan.FromSeconds(2),
				TaskOwner = workloadDay,
				ValidatedTasks = 110
			};

			var taskOwnerPeriod = new TaskOwnerPeriod(dateOnly, new ITaskOwner[] { validatedVolumeDay }, TaskOwnerPeriodType.Other);
			historicalData.Stub(
				x => x.Fetch(quickForecasterWorkloadParams.WorkLoad, quickForecasterWorkloadParams.HistoricalPeriod))
				.Return(taskOwnerPeriod);
			var target = new QuickForecasterWorkload(historicalData,
				MockRepository.GenerateMock<IFutureData>(),
				forecastMethodProvider,
				MockRepository.GenerateMock<IForecastingTargetMerger>(), outlierRemover);
			
			target.Execute(quickForecasterWorkloadParams);
			outlierRemover.AssertWasCalled(x => x.RemoveOutliers(taskOwnerPeriod, method));
		}
	}
}