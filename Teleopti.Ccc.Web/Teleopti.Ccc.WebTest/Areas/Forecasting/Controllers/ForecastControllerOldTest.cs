using System;
using System.Collections.Generic;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class ForecastControllerOldTest
	{
		[Test]
		public void ShouldForecast()
		{
			var forecastCreator = MockRepository.GenerateMock<IForecastCreator>();

			var scenarioId = Guid.NewGuid();
			var forecastInput = new ForecastInput
			{
				ForecastStart = new DateTime(2014, 4, 1),
				ForecastEnd = new DateTime(2014, 4, 29),
				Workload = new ForecastWorkloadInput(),
				ScenarioId = scenarioId
			};
			var scenario = new Scenario("test1").WithId(scenarioId);
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var target = new ForecastController(forecastCreator, null, null, scenarioRepository, null, null, null,
				null, null, null, null);
			var forecast = new ForecastModel
			{
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2016, 05, 01),
						Tasks = 10
					}
				}
			};

			var period = new DateOnlyPeriod(new DateOnly(forecastInput.ForecastStart), new DateOnly(forecastInput.ForecastEnd));
			forecastCreator.Stub(x => x.CreateForecastForWorkload(period, forecastInput.Workload, scenario)).Return(forecast);

			var result = (OkNegotiatedContentResult<ForecastModel>)target.Forecast(forecastInput);
			forecastCreator.AssertWasCalled(x => x.CreateForecastForWorkload(period, forecastInput.Workload, scenario));
			result.Should().Be.OfType<OkNegotiatedContentResult<ForecastModel>>();
		}
	}
}
