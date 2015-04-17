using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Core
{
	public class PreForecasterTest
	{
		[Test]
		public void ShouldMeasureAndForecast()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var workload = skill.WorkloadCollection.Single();
			var preForecastInput = new PreForecastInput
			{
				WorkloadId = workload.Id.Value,
				ForecastStart = new DateTime(2015, 1, 1),
				ForecastEnd = new DateTime(2015, 1, 31)
			};
			workloadRepository.Stub(x => x.Get(preForecastInput.WorkloadId)).Return(workload);
			var quickForecastWorkloadEvaluator = MockRepository.GenerateMock<IQuickForecastWorkloadEvaluator>();
			var historicalPeriodProvider = new HistoricalPeriodProvider(new MutableNow(new DateTime(2014, 12, 20, 0, 0, 0, DateTimeKind.Utc)));
			quickForecastWorkloadEvaluator.Stub(x => x.Measure(workload, historicalPeriodProvider.PeriodForEvaluate()))
				.Return(new WorkloadAccuracy
				{
					Accuracies =
						new[]
						{
							new MethodAccuracy {Number = 89, MethodId = ForecastMethodType.TeleoptiClassic, IsSelected = false},
							new MethodAccuracy {Number = 92, MethodId = ForecastMethodType.TeleoptiClassicWithTrend, IsSelected = true}
						}
				});
			var preForecastWorkload = MockRepository.GenerateMock<IPreForecastWorkload>();
			var dictionary = new Dictionary<DateOnly, IDictionary<ForecastMethodType, double>>();
			var oneDay = new Dictionary<ForecastMethodType, double> {{ForecastMethodType.TeleoptiClassic, 80d},{ForecastMethodType.TeleoptiClassicWithTrend, 90d}};
			var dateOnly = new DateOnly(2015,1,1);
			dictionary.Add(dateOnly, oneDay);
			preForecastWorkload.Stub(x => x.PreForecast(workload, new DateOnlyPeriod(new DateOnly(preForecastInput.ForecastStart), new DateOnly(preForecastInput.ForecastEnd)))).Return(dictionary);
			var target = new PreForecaster(preForecastWorkload, quickForecastWorkloadEvaluator, workloadRepository, historicalPeriodProvider);

			
			var result = target.MeasureAndForecast(preForecastInput);

			result.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Name.Should().Be.EqualTo(workload.Name);
			result.SelectedForecastMethod.Should().Be.EqualTo(ForecastMethodType.TeleoptiClassicWithTrend);
			result.ForecastMethods.Any(x=>(int)x.AccuracyNumber==89&&x.ForecastMethodType==ForecastMethodType.TeleoptiClassic).Should().Be.True();
			result.ForecastMethods.Any(x => (int)x.AccuracyNumber == 92 && x.ForecastMethodType == ForecastMethodType.TeleoptiClassicWithTrend).Should().Be.True();
			dynamic forecastDayViewModel = result.ForecastDayViewModels[0];
			((object)forecastDayViewModel.date).Should().Be.EqualTo(dateOnly.Date);
			((object)forecastDayViewModel.vh).Should().Be.EqualTo(0);
			((object)forecastDayViewModel.v0).Should().Be.EqualTo(80d);
			((object)forecastDayViewModel.v1).Should().Be.EqualTo(90d);
		}
	}
}