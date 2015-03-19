using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

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
			var target = new ForecastController(quickForecastEvaluator, skillRepository, null);
			var skills = target.Skills();
			skills.Single().Id.Should().Be.EqualTo(skill1.Id.Value);
			skills.Single().Name.Should().Be.EqualTo(skill1.Name);
			var workload = skill1.WorkloadCollection.Single();
			skills.Single().Workloads.Single().Id.Should().Be.EqualTo(workload.Id.Value);
			skills.Single().Workloads.Single().Name.Should().Be.EqualTo(workload.Name);
		}

		[Test]
		public void ShouldMeasureForecast()
		{
			var quickForecastEvaluator = MockRepository.GenerateMock<IQuickForecastEvaluator>();
			quickForecastEvaluator.Stub(x => x.MeasureForecastForAllSkills())
				.Return(new[] {new ForecastingAccuracy {Accuracy = 90.2}});
			var target = new ForecastController(quickForecastEvaluator, null, null);

			var result = target.MeasureForecast();

			result.Result[0].Accuracy.Should().Be.EqualTo(90.2);
		}

		[Test]
		public void ShouldForecast()
		{
			var now = new Now();
			var expectedFuturePeriod = new DateOnlyPeriod(new DateOnly(now.UtcDateTime()), new DateOnly(now.UtcDateTime().AddYears(1)));
			var quickForecastCreator = MockRepository.GenerateMock<IQuickForecastCreator>();
			
			var target = new ForecastController(null, null, quickForecastCreator);
			var workloads = new[] {Guid.NewGuid() };
			var result = target.Forecast(new QuickForecastInputModel
			{
				ForecastStart = expectedFuturePeriod.StartDate,
				ForecastEnd = expectedFuturePeriod.EndDate,
				Workloads = workloads
			});
			result.Result.Should().Be.True();
			quickForecastCreator.AssertWasCalled(x => x.CreateForecastForWorkloads(expectedFuturePeriod, workloads));
		}
	}


}
