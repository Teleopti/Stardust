using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class ForecastControllerTest
	{
		[Test]
		public void ShouldCallQuickForecasterWithHistorialDataForOneYear()
		{
			var now = new Now();
			var expectedFuturePeriod = new DateOnlyPeriod(new DateOnly(now.UtcDateTime()),
				new DateOnly(now.UtcDateTime().AddYears(1)));
			var expectedHistoricalPeriod = new DateOnlyPeriod(new DateOnly(now.UtcDateTime().AddYears(-1)),
				new DateOnly(now.UtcDateTime()));
			var skillRepository = MockRepository.GenerateStub<ISkillRepository>();
			var skill = SkillFactory.CreateSkill("_");
			var workload = new Workload(skill);
			workload.AddQueueSource(QueueSourceFactory.CreateQueueSource());
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill});
			var quickForecaster = MockRepository.GenerateMock<IQuickForecaster>();
			var target = new ForecastController(new QuickForecastForAllSkills(quickForecaster, skillRepository), new OneYearHistoryForecastPeriodCalculator(now), null, MockRepository.GenerateMock<INow>());

			target.QuickForecast(new QuickForecastInputModel
			{
				ForecastStart = expectedFuturePeriod.StartDate,
				ForecastEnd = expectedFuturePeriod.EndDate
			});

			quickForecaster.AssertWasCalled(x => x.Execute(skill, expectedHistoricalPeriod, expectedFuturePeriod));
		}

		[Test]
		public void ShouldMeasureQuickForecasterMethod()
		{
			var now = new Now();
			var yesterday = now.UtcDateTime().AddDays(-1);
			var expectedFuturePeriod = new DateOnlyPeriod(new DateOnly(yesterday.AddYears(-1)), new DateOnly(yesterday));
			var expectedHistoricalPeriod = new DateOnlyPeriod(new DateOnly(yesterday.AddYears(-2)), new DateOnly(yesterday.AddYears(-1)));

			var quickForecastForAllSkills = MockRepository.GenerateMock<IQuickForecastForAllSkills>();
			quickForecastForAllSkills.Stub(x => x.MeasureForecast(expectedHistoricalPeriod, expectedFuturePeriod)).Return(0.96);
			var target = new ForecastController(quickForecastForAllSkills, new OneYearHistoryForecastPeriodCalculator(now), null, now);

			var result=target.MeasureForecast();

			result.Should().Be.EqualTo(0.96);
		}

		[Test]
		public void ShouldGetTheCurrentIdentityName()
		{
			var target = new ForecastController(null, null, new FakeCurrentIdentity("Pelle"), MockRepository.GenerateMock<INow>());
			dynamic result = target.GetThatShouldBeInAMoreGenericControllerLaterOn();
			Assert.AreEqual("Pelle", result.UserName);
		}
	}
}
