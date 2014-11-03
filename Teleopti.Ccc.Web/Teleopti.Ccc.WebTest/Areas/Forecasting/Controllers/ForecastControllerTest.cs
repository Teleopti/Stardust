﻿using NUnit.Framework;
using Rhino.Mocks;
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
			var target = new ForecastController(new QuickForecastForAllSkills(quickForecaster, skillRepository), new OneYearHistoryForecastPeriodCalculator(now));

			target.QuickForecast(new QuickForecastInputModel
			{
				ForecastStart = expectedFuturePeriod.StartDate,
				ForecastEnd = expectedFuturePeriod.EndDate
			});

			quickForecaster.AssertWasCalled(x => x.Execute(skill, expectedHistoricalPeriod, expectedFuturePeriod));
		}
	}
}
