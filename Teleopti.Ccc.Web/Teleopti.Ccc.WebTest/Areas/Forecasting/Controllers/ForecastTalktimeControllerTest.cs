using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastTalkTimeControllerTest : IExtendSystem
	{
		public ForecastController Target;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeStatisticRepository StatisticRepository;
		public FakeWorkloadRepository WorkloadRepository;

		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<ForecastController>();
		}

		[Test]
		public void ShouldUseTeleoptiClassicShortTermMethodToForecast()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-7).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = 100
				},
				new StatisticTask
				{
					Interval = openDay.AddDays(-8).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = 400
				}
			});

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date.AddDays(1),
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
			var forecastedDay1 = result.Content.ForecastDays.First();
			var forecastedDay2 = result.Content.ForecastDays.Last();
			result.Content.TalkTimeForecastMethodId.Should().Be(ForecastMethodType.TeleoptiClassicShortTerm);
			forecastedDay1.Should().Not.Be.EqualTo(forecastedDay2);

		}
	}
}
