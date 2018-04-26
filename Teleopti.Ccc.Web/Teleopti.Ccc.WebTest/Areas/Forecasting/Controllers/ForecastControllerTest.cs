using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastControllerTest
	{
		public ForecastController Target;

		[Test]
		public void ShouldCreateAndReturnForecastWithOutSaving()
		{
			ForecastInput forecastInput = new ForecastInput()
			{
				ForecastStart = new DateTime(2018,05,01),
				ForecastEnd = new DateTime(2018, 05, 02),
				ScenarioId = Guid.NewGuid(),
				Workload = new ForecastWorkloadInput {}
			};
			var result = Target.Forecast(forecastInput);
		}
	}
}
