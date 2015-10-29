using System;
using System.Linq;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Forecasting
{
	[Binding]
	public class ForecasterSteps
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

		[When(@"I choose scenario '(.*)'")]
		public void WhenIChooseScenario(string scenario)
		{
			Browser.Interactions.Click(".wfm-card-selected .scenario-select");
			Browser.Interactions.ClickContaining(".wfm-card-selected .scenario-select option", scenario);
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
			Browser.Interactions.Click(".wfm-card-selected .wfm-btn-invis-default.forecast-workload");
			Browser.Interactions.AssertExists("span.startDate");
			if (!ScenarioContext.Current.ContainsKey("startdate"))
				ScenarioContext.Current.Add("startdate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.startDate"))));
			if (!ScenarioContext.Current.ContainsKey("enddate"))
				ScenarioContext.Current.Add("enddate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.endDate"))));
			Browser.Interactions.Click(".do-forecast");
		}

		[Given(@"forecast result has loaded")]
		[When(@"forecast result has loaded")]
		[Then(@"forecast result has loaded")]
		public void ThenForecastResultHasLoaded()
		{
			WhenISelectTheFirstDayInTheForecastChart();
		}

		[When(@"I use default forecast period and continue with advanced")]
		public void WhenIClickQuickforecaster()
		{
			ScenarioContext.Current.Add("startdate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.startDate"))));
			ScenarioContext.Current.Add("enddate", new DateOnly(DateTime.Parse(Browser.Interactions.GetText("span.endDate"))));
			Browser.Interactions.Click(".next-step-advanced");
		}

		[Then(@"there is forecast data for default period for")]
		public void ThenThereIsForecastDataForDefaultPeriodFor(Table table)
		{
			var forecastData = table.CreateInstance<ForecastData>();
			checkForecastResult(forecastData.Workload, forecastData.Scenario);
		}

		[Then(@"there is forecast data for default period for '(.*)'")]
		public void ThenThereIsForecastDataForDefaultPeriodFor(string workload)
		{
			checkForecastResult(workload, DefaultScenario.Scenario.Description.Name);
		}

		private static void checkForecastResult(string workload, string scenario)
		{
			var choosenPeriod = new DateOnlyPeriod((DateOnly) ScenarioContext.Current["startdate"],
				((DateOnly) ScenarioContext.Current["enddate"]).AddDays(-1));
			GlobalUnitOfWorkState.UnitOfWorkAction(uow =>
			{
				var workloadId = new WorkloadRepository(uow).LoadAll().SingleOrDefault(x => x.Name == workload).Id;
				var allSkillDays = new SkillDayRepository(uow).LoadAll();

				allSkillDays = allSkillDays.Where(x => x.Scenario.Description.Name == scenario).ToList();

				foreach (var dateOnly in choosenPeriod.DayCollection())
				{
					var skillDay = allSkillDays.SingleOrDefault(x => x.CurrentDate == dateOnly);
					skillDay.Should().Not.Be.Null();
					var taskPeriods = skillDay.WorkloadDayCollection.SingleOrDefault(x => x.Workload.Id == workloadId).TaskPeriodList;
					taskPeriods.Count.Should().Be.EqualTo(96);
				}
			});
		}

		private static void checkNoForecastResult(string workload, string scenario)
		{
			var choosenPeriod = new DateOnlyPeriod((DateOnly)ScenarioContext.Current["startdate"],
				((DateOnly)ScenarioContext.Current["enddate"]).AddDays(-1));
			GlobalUnitOfWorkState.UnitOfWorkAction(uow =>
			{
				var workloadId = new WorkloadRepository(uow).LoadAll().SingleOrDefault(x => x.Name == workload).Id;
				var allSkillDays = new SkillDayRepository(uow).LoadAll();

				allSkillDays = allSkillDays.Where(x => x.Scenario.Description.Name == scenario).ToList();

				foreach (var dateOnly in choosenPeriod.DayCollection())
				{
					var skillDay = allSkillDays.SingleOrDefault(x => x.CurrentDate == dateOnly);
					skillDay.Should().Not.Be.Null();
					skillDay.WorkloadDayCollection.SingleOrDefault(x => x.Workload.Id == workloadId).TaskPeriodList.Single().Task.Tasks
						.Should().Be.EqualTo(0);
				}
			});
		}

		[Then(@"there is no forecast data for default period for '(.*)'")]
		public void ThenThereIsNoForecastDataForDefaultPeriodFor(string workload)
		{
			checkNoForecastResult(workload, DefaultScenario.Scenario.Description.Name);
		}

		[Then(@"there is no forecast data for default period for")]
		public void ThenThereIsNoForecastDataForDefaultPeriodFor(Table table)
		{
			var forecastData = table.CreateInstance<ForecastData>();
			checkNoForecastResult(forecastData.Workload, forecastData.Scenario);
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

		[When(@"I select to modify the forecast")]
		public void WhenISelectToModifyTheForecast()
		{
			Browser.Interactions.Click(".forecast-modify-button");
		}

		[When(@"I choose to add a campaign")]
		public void WhenIChooseToAddACampaign()
		{
			Browser.Interactions.Click(".forecast-add-campaign-button");
		}

		[When(@"I select to do a manual change")]
		public void WhenISelectToDoAManualChange()
		{
			Browser.Interactions.Click(".forecast-override-tasks-button");
		}

		[When(@"I enter '(.*)' calls per day")]
		public void WhenIEnterCallsPerDay(string calls)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".forecast-manual-change-input", calls);
		}

		[When(@"I increase the calls by (.*) percent")]
		public void WhenIIncreaseTheCallsByPercent(string percent)
		{
			Browser.Interactions.FillWith(".forecast-campaign-input", percent);
			var callCount = Browser.Interactions.GetText(".forecast-campaign-calls-count");
			Browser.Interactions.AssertNoContains(".forecast-campaign-calls-count", ".forecast-campaign-totalcalls-count", callCount);
		}

		[When(@"I apply the campaign")]
		public void WhenIApplyTheCampaign()
		{
			Browser.Interactions.Click(".forecast-apply-campaign-button");
		}

		[When(@"I apply the manual change")]
		public void WhenIApplyTheManualChange()
		{
			Browser.Interactions.Click(".forecast-apply-override-tasks-button");
		}
		
		[When(@"I should see that the total calls for the first day has doubled")]
		[Then(@"I should see that the total calls for the first day has doubled")]
		public void ThenIShouldSeeThatTheTotalCallsForTheFirstDayHasDoubled()
		{
			WhenForecastHasSucceeded();
			Browser.Interactions.AssertExists(".wfm-card-selected .c3");
			Browser.Interactions.AssertJavascriptResultContains(
				"var v1= parseFloat(angular.element(document.querySelector('.c3')).scope().chart.data.values('vtc')[0]);" +
				"var v2= (parseFloat(angular.element(document.querySelector('.c3')).scope().chart.data.values('vc')[0])*2);" +
				"return (v1==v2) + '  ' + v1+'  '+ v2;"
				, "true");
		}

		[Then(@"I should see that the total calls for the first day is '(.*)'")]
		public void ThenIShouldSeeThatTheTotalCallsForTheFirstDayIs(string calls)
		{
			WhenForecastHasSucceeded();
			Browser.Interactions.AssertExists(".wfm-card-selected .c3");
			Browser.Interactions.AssertJavascriptResultContains(
					string.Format(
					"var numberOfCalls = parseFloat(angular.element(document.querySelector('.c3')).scope().chart.data.values('vtc')[0]);" +
					"return (numberOfCalls=={0}) + '  ' + numberOfCalls';"
					, calls)
				, "true");
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

		[When(@"I choose to add a new skill")]
		public void GivenIChooseToAddANewSkill()
		{
			Browser.Interactions.Click(".skill-create-button");
		}

		[When(@"I input the new skill with")]
		public void WhenIInputTheNewSkillWith(Table table)
		{
			var newSkill = table.CreateInstance<NewSkill>();
			Browser.Interactions.FillWith(".skill-name input", newSkill.Name);
			Browser.Interactions.FillWith(".skill-service-level-percent input", newSkill.ServiceLevelPercent);
			Browser.Interactions.FillWith(".skill-service-level-seconds input", newSkill.ServiceLevelSeconds);
			Browser.Interactions.FillWith(".skill-shrinkage input", newSkill.Shrinkage);
			Browser.Interactions.SelectOptionByTextUsingJQuery(".skill-activity select", newSkill.Activity);
			Browser.Interactions.SelectOptionByTextUsingJQuery(".skill-timezone select", newSkill.Timezone);
			Browser.Interactions.ClickContaining(".skill-queues .big-table-wrapper .ui-grid-cell-contents", newSkill.Queues);
		}

		[When(@"I save the new skill")]
		public void WhenISaveTheNewSkill()
		{
			Browser.Interactions.Click(".skill-save");
		}

		[Then(@"I should see the new skill '(.*)' in the list")]
		public void ThenIShouldSeeTheNewSkillInTheList(string name)
		{
			Browser.Interactions.AssertAnyContains(".wfm-card-list wfm-card card-header", name);
		}

		[When(@"I input opening hours with")]
		public void WhenIInputOpeningHoursWith(Table table)
		{
			var openingHours = table.CreateInstance<OpeningHours>();
		}
	}

	public class OpeningHours
	{

	}

	public class NewSkill
	{
		public string Name { get; set; }
		public string Activity { get; set; }
		public string Timezone { get; set; }
		public string Queues { get; set; }
		public string ServiceLevelPercent { get; set; }
		public string ServiceLevelSeconds { get; set; }
		public string Shrinkage { get; set; }
	}

	public class ForecastData
	{
		public string Workload { get; set; }
		public string Scenario { get; set; }
	}
}
