using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class ForecastControllerTest
	{
		[Test]
		public void ShouldGetSkillsAndWorkloads()
		{
			var quickForecastEvaluator = MockRepository.GenerateMock<IQuickForecastEvaluator>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill1.SetId(Guid.NewGuid());
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill1});
			var target = new ForecastController(null, skillRepository, null);
			var skills = target.Skills();
			skills.Single().Id.Should().Be.EqualTo(skill1.Id.Value);
			skills.Single().Name.Should().Be.EqualTo(skill1.Name);
			var workload = skill1.WorkloadCollection.Single();
			skills.Single().Workloads.Single().Id.Should().Be.EqualTo(workload.Id.Value);
			skills.Single().Workloads.Single().Name.Should().Be.EqualTo(workload.Name);
		}

		[Test]
		public void ShouldPreForecast()
		{
			var preForecaster = MockRepository.GenerateMock<IPreForecaster>();
			var preForecastInputModel = new PreForecastInput();
			var workloadForecastingViewModel = new WorkloadForecastViewModel();
			preForecaster.Stub(x => x.MeasureAndForecast(preForecastInputModel)).Return(workloadForecastingViewModel);
			var target = new ForecastController(null, null, preForecaster);

			var result = target.PreForecast(preForecastInputModel);

			result.Result.Should().Be.EqualTo(workloadForecastingViewModel);
		}
	}


}
