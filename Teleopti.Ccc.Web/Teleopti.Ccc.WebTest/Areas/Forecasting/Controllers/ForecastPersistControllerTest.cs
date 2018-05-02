using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastPersistControllerTest
	{
		public ForecastController Target;
		public FakeSkillDayRepository SkillDayRepository;

		[Test]
		public void ShouldSaveForecast()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var scenario = ScenarioFactory.CreateScenario("Default", true, true);
			IList<ForecastDayModel> forecastDays = new List<ForecastDayModel>{new ForecastDayModel()
			{
				Date = new DateOnly(2018,05,02),
				Tasks = 10,
				TaskTime = 60,
				AfterTaskTime = 60
			}};
			var forecastResult = new ForecastPersistModel()
			{
				WorkloadId = Guid.NewGuid(),
				ScenarioId = Guid.NewGuid(),
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);
			SkillDayRepository.FindRange(new DateOnly(2018, 05, 02).ToDateOnlyPeriod(), skill, scenario);
		}
	}
}
