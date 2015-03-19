using System;
using System.Linq;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Forecasting
{
	[Binding]
	public class QuickForecasterSteps
	{
		[Then(@"there is a SkillDay for '(.*)'")]
		public void ThenThereIsASkillDayFor(string date)
		{
			var theDate = new DateOnly(DateTime.Parse(date));
			GlobalUnitOfWorkState.UnitOfWorkAction(uow =>
				new SkillDayRepository(uow).LoadAll().Any(x => x.CurrentDate == theDate).Should().Be.True());
		}

		[When(@"Quickforecast has succeeded")]
		public void WhenQuickforecastHasSucceeded()
		{
			Browser.Interactions.AssertExists("div.success");
		}

		[Given(@"there is no forecast data")]
		public void GivenThereIsNoForecastData()
		{
			GlobalUnitOfWorkState.UnitOfWorkAction(uow =>
				new SkillDayRepository(uow).LoadAll().Any().Should().Be.False());
		}

		[When(@"I am viewing quick forecast page")]
		public void WhenIAmViewingQuickForecastPage()
		{
			TestControllerMethods.Logon();
			Navigation.GotoQuickForecaster();
		}

		[When(@"I select skill '(.*)'")]
		public void WhenISelectSkill(string skill)
		{
			Browser.Interactions.ClickContaining(".skill", skill);
		}

		[When(@"I select workload '(.*)'")]
		public void WhenISelectWorkload(string workload)
		{
			Browser.Interactions.ClickContaining(".workload", workload);
		}

		[When(@"I choose to forecast the selected targets")]
		public void WhenIChooseToForecastTheSelectedTargets()
		{
			Browser.Interactions.Click(".apply");
		}


		[When(@"I use default forecast period and continue")]
		public void WhenIClickQuickforecaster()
		{
			ScenarioContext.Current.Add("startdate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.startDate"))));
			ScenarioContext.Current.Add("enddate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.endDate"))));
			Browser.Interactions.Click(".next-step");
		}

		[Then(@"there is forecast data for default period for '(.*)'")]
		public void ThenThereIsForecastDataForDefaultPeriodFor(string workload)
		{
			var choosenPeriod = new DateOnlyPeriod((DateOnly)ScenarioContext.Current["startdate"], ((DateOnly)ScenarioContext.Current["enddate"]).AddDays(-1));
			GlobalUnitOfWorkState.UnitOfWorkAction(uow =>
			{
				var workloadId = new WorkloadRepository(uow).LoadAll().SingleOrDefault(x => x.Name == workload).Id;
				var allSkillDays = new SkillDayRepository(uow).LoadAll();

				foreach (var dateOnly in choosenPeriod.DayCollection())
				{
					var skillDay = allSkillDays.SingleOrDefault(x => x.CurrentDate == dateOnly);
					skillDay.Should().Not.Be.Null();
					skillDay.WorkloadDayCollection.SingleOrDefault(x => x.Workload.Id == workloadId)
						.Should().Not.Be.Null();
				}
			});
		}

		[Then(@"there is no forecast data for default period for '(.*)'")]
		public void ThenThereIsNoForecastDataForDefaultPeriodFor(string workload)
		{
			var choosenPeriod = new DateOnlyPeriod((DateOnly)ScenarioContext.Current["startdate"], ((DateOnly)ScenarioContext.Current["enddate"]).AddDays(-1));
			GlobalUnitOfWorkState.UnitOfWorkAction(uow =>
			{
				var workloadId = new WorkloadRepository(uow).LoadAll().SingleOrDefault(x => x.Name == workload).Id;
				var allSkillDays = new SkillDayRepository(uow).LoadAll();

				foreach (var dateOnly in choosenPeriod.DayCollection())
				{
					var skillDay = allSkillDays.SingleOrDefault(x => x.CurrentDate == dateOnly);
					skillDay.Should().Not.Be.Null();
					skillDay.WorkloadDayCollection.SingleOrDefault(x => x.Workload.Id == workloadId)
						.Should().Be.Null();
				}
			});
		}



		[Then(@"there are SkillDays for default period")]
		public void ThenThereAreSkillDaysForDefaultPeriod()
		{
			var choosenPeriod = new DateOnlyPeriod((DateOnly)ScenarioContext.Current["startdate"], ((DateOnly)ScenarioContext.Current["enddate"]).AddDays(-1));
			GlobalUnitOfWorkState.UnitOfWorkAction(uow =>
			{
				var allSkillDays = new SkillDayRepository(uow).LoadAll();

				foreach (var dateOnly in choosenPeriod.DayCollection())
				{
					allSkillDays.SingleOrDefault(x => x.CurrentDate == dateOnly)
						.Should().Not.Be.Null();
				}
			});
		}

		[Then(@"I should see the accuracy for the forecast method")]
		public void ThenIShouldSeeTheAccuracyForTheForecastMethod()
		{
			Browser.Interactions.AssertAnyContains(".forecast-relative-error","%");
		}

		[Then(@"I should see the total forecasting accuracy")]
		public void ThenIShouldSeeTheTotalForecastingAccuracy()
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".total-accuracy", "%");
		}

		[Then(@"I should see the forecasting accuracy for '(.*)'")]
		public void ThenIShouldSeeTheForecastingAccuracyFor(string workload)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".workload:contains(" + workload + ") .workload-accuracy", "%");
		}

		[Then(@"I should see a message of no historical data for measurement")]
		public void ThenIShouldSeeAMessageOfNoHistoricalDataForMeasurement()
		{
			Browser.Interactions.AssertAnyContains(".forecast-relative-error", "Not enough historical data for measuring.");
		}

	}
}
