using System;
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
		public void ShouldQuickForecastForOneWorkload()
		{
			var workloadId = Guid.NewGuid();
			var workloads = new[] { workloadId };
			var forecastStart = new DateTime(2015, 1, 1);
			var forecastEnd = new DateTime(2015, 2, 1);
			var futurePeriod = new DateOnlyPeriod(new DateOnly(forecastStart), new DateOnly(forecastEnd));
			var quickForecastCreator = MockRepository.GenerateMock<IQuickForecastCreator>();
			quickForecastCreator.Stub(x => x.CreateForecastForWorkloads(futurePeriod, workloads)).Return(new[] { new ForecastingAccuracy { Id = workloadId, Accuracy=0.5d} });
			var target = new ForecastController(quickForecastCreator, MockRepository.GenerateMock<ICurrentIdentity>());
			
			var result = target.QuickForecast(new QuickForecastInputModel
			{
				ForecastStart = forecastStart,
				ForecastEnd = forecastEnd,
				Workloads = workloads
			});

			result.Result[0].Accuracy.Should().Be.EqualTo(0.5d);
			result.Result[0].Id.Should().Be.EqualTo(workloadId);
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
			var target = new ForecastController(new QuickForecastCreator(quickForecaster, skillRepository, now), null);

			var result = target.QuickForecast(new QuickForecastInputModel
			{
				ForecastStart = expectedFuturePeriod.StartDate,
				ForecastEnd = expectedFuturePeriod.EndDate
			});

			result.Result[0].IsAll.Should().Be.EqualTo(true);
			result.Result[0].Accuracy.Should().Be.EqualTo(0);
			quickForecaster.AssertWasCalled(x => x.ForecastForSkill(skill, expectedFuturePeriod, expectedHistoricPeriod));
		}

		[Test]
		public void ShouldGetTheCurrentIdentityName()
		{
			var target = new ForecastController(null, new FakeCurrentIdentity("Pelle"));
			dynamic result = target.GetThatShouldBeInAMoreGenericControllerLaterOn();
			Assert.AreEqual("Pelle", result.UserName);
		}
	}
}
