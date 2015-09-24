using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Forecasting
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

		[Given(@"Forecast has succeeded")]
		[When(@"Forecast has succeeded")]
		[Then(@"Forecast has succeeded")]
		public void WhenForecastHasSucceeded()
		{
			Browser.Interactions.AssertExistsUsingJQuery(".wfm-card-list md-card:last card-header .mdi-check");
		}

		[Given(@"there is no forecast data")]
		public void GivenThereIsNoForecastData()
		{
			GlobalUnitOfWorkState.UnitOfWorkAction(uow =>
				new SkillDayRepository(uow).LoadAll().Any().Should().Be.False());
		}
		
		[Given(@"I am viewing quick forecast page")]
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
			Browser.Interactions.AssertExistsUsingJQuery(".skill:contains('" + skill + "').selected");
			Browser.Interactions.AssertExistsUsingJQuery(".workload:first.selected");
			Browser.Interactions.AssertExistsUsingJQuery(".workload:last.selected");
		}

		[Given(@"I select workload '(.*)'")]
		[When(@"I select workload '(.*)'")]
		public void WhenISelectWorkload(string workload)
		{
			Browser.Interactions.ClickContaining("card-header span b", workload);
		}

		[When(@"I continue with advanced")]
		public void WhenIContinueWithAdvanced()
		{
			Browser.Interactions.Click(".wfm-card-selected .wfm-btn");
			Browser.Interactions.AssertExists(".back");
		}

		[When(@"I choose to forecast the selected targets")]
		public void WhenIChooseToForecastTheSelectedTargets()
		{
			Browser.Interactions.Click(".apply");
		}

		[When(@"I use default forecast period and forecast for all")]
		public void WhenIUseDefaultForecastPeriodAndForecastForAll()
		{
			Browser.Interactions.Click(".forecast-create-button");
			ScenarioContext.Current.Add("startdate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.startDate"))));
			ScenarioContext.Current.Add("enddate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.endDate"))));
			Browser.Interactions.Click(".do-forecast");
		}

		[Given(@"I use default forecast period and forecast for one workload")]
		[When(@"I use default forecast period and forecast for one workload")]
		[Then(@"I use default forecast period and forecast for one workload")]
		public void WhenIUseDefaultForecastPeriodAndForecastForOneWorkload()
		{
			Browser.Interactions.Click(".wfm-card-selected .forecast-workload");
			if (!ScenarioContext.Current.ContainsKey("startdate"))
				ScenarioContext.Current.Add("startdate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.startDate"))));
			if (!ScenarioContext.Current.ContainsKey("enddate"))
				ScenarioContext.Current.Add("enddate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.endDate"))));
			Browser.Interactions.Click(".do-forecast");
		}


		[When(@"I use default forecast period and continue with advanced")]
		public void WhenIClickQuickforecaster()
		{
			ScenarioContext.Current.Add("startdate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.startDate"))));
			ScenarioContext.Current.Add("enddate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.endDate"))));
			Browser.Interactions.Click(".next-step-advanced");
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
					var taskPeriods = skillDay.WorkloadDayCollection.SingleOrDefault(x => x.Workload.Id == workloadId).TaskPeriodList;
					taskPeriods.Count.Should().Be.EqualTo(96);
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
					skillDay.WorkloadDayCollection.SingleOrDefault(x => x.Workload.Id == workloadId).TaskPeriodList.Single().Task.Tasks
						.Should().Be.EqualTo(0);
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

		[When(@"I select the first day in the forecast chart")]
		public void WhenISelectTheFirstDayInTheForecastChart()
		{
			Browser.Interactions.Click(".c3-event-rect-0");
		}

		[When(@"I choose to add a campaign")]
		public void WhenIChooseToAddACampaign()
		{
			Browser.Interactions.Click(".forecast-add-campaign-button");
		}

		[When(@"I increase the calls by (.*) percent")]
		public void WhenIIncreaseTheCallsByPercent(int p0)
		{
			Browser.Interactions.FillWith(".forecast-campaign-input", "100");
			var callCount = Browser.Interactions.GetText(".forecast-campaign-calls-count");
			Browser.Interactions.AssertNoContains(".forecast-campaign-calls-count", ".forecast-campaign-totalcalls-count", callCount);
		}

		[When(@"I apply the campaign")]
		public void WhenIApplyTheCampaign()
		{
			Browser.Interactions.Click(".forecast-apply-campaign-button");
		}

		[When(@"I should see that the total calls for the first day has doubled")]
		[Then(@"I should see that the total calls for the first day has doubled")]
		public void ThenIShouldSeeThatTheTotalCallsForTheFirstDayHasDoubled()
		{
			var totalCalls = Browser.Interactions.Javascript("return angular.element(document.querySelector('.c3')).scope().chart.data.values('vtc')[0];");
			var calls = Browser.Interactions.Javascript("return angular.element(document.querySelector('.c3')).scope().chart.data.values('vc')[0];");
			Assert.AreEqual(Math.Round(double.Parse(calls) * 2, 1), Math.Round(double.Parse(totalCalls), 1));
		}

		[Given(@"I am viewing the forecast chart")]
		public void GivenIAmViewingTheForecastChart()
		{
			Browser.Interactions.ClickUsingJQuery("card-header:first()");
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

		[Then(@"I should see the forecasting accuracy for skill '(.*)'")]
		public void ThenIShouldSeeTheForecastingAccuracyForSkill(string skill)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".skill:contains(" + skill + ") .skill-accuracy", "%");
		}

		[Then(@"I should see the forecasting accuracy for workload '(.*)'")]
		public void ThenIShouldSeeTheForecastingAccuracyForWorkload(string workload)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".workload:contains(" + workload + ") .workload-accuracy", "%");
		}

		[Then(@"I should see no accuracy for total")]
		public void ThenIShouldSeeNoAccuracyForTotal()
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".total-accuracy", "-%");
		}

		[Then(@"I should see no accuracy for skill '(.*)'")]
		public void ThenIShouldSeeNoAccuracyForSkill(string skill)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".skill:contains(" + skill + ") .skill-accuracy", "-%");
		}

		[Then(@"I should see no accuracy for workload '(.*)'")]
		public void ThenIShouldSeeNoAccuracyForWorkload(string workload)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".workload:contains(" + workload + ") .workload-accuracy", "-%");
		}
	}
}
