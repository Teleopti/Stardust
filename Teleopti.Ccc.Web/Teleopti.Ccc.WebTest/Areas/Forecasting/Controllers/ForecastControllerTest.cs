using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
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
			var quickForecastCreator = MockRepository.GenerateMock<IQuickForecastCreator>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill1.SetId(Guid.NewGuid());
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill1});
			var target = new ForecastController(quickForecastCreator, MockRepository.GenerateMock<ICurrentIdentity>(), skillRepository);
			var skills = target.Skills();
			skills.Single().Id.Should().Be.EqualTo(skill1.Id.Value);
			skills.Single().Name.Should().Be.EqualTo(skill1.Name);
			var workload = skill1.WorkloadCollection.Single();
			skills.Single().Workloads.Single().Id.Should().Be.EqualTo(workload.Id.Value);
			skills.Single().Workloads.Single().Name.Should().Be.EqualTo(workload.Name);
		}

		[Test]
		public void ShouldCallQuickForecasterAndReturnAccuracy()
		{
			var now = new Now();
			var expectedFuturePeriod = new DateOnlyPeriod(new DateOnly(now.UtcDateTime()),
				new DateOnly(now.UtcDateTime().AddYears(1)));
			var expectedHistoricPeriod = new DateOnlyPeriod(new DateOnly(now.LocalDateOnly().Date.AddYears(-2)), now.LocalDateOnly());
			var skillRepository = MockRepository.GenerateStub<ISkillRepository>();
			var skill = SkillFactory.CreateSkill("_");
			var workload = new Workload(skill);
			workload.AddQueueSource(QueueSourceFactory.CreateQueueSource());
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill});
			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var target = new ForecastController(new QuickForecastCreator(quickForecaster, skillRepository, now), null, null);

			var result = target.MeasureForecast(new QuickForecastInputModel
			{
				ForecastStart = expectedFuturePeriod.StartDate,
				ForecastEnd = expectedFuturePeriod.EndDate
			});

			result.Result[0].IsAll.Should().Be.EqualTo(true);
			result.Result[0].Accuracy.Should().Be.EqualTo(0);
			quickForecaster.AssertWasCalled(x => x.ForecastForSkill(skill, expectedFuturePeriod, expectedHistoricPeriod));
		}
	}
}
