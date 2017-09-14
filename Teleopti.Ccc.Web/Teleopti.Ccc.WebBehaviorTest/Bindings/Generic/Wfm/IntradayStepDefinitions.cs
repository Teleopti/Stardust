using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class IntradayStepDefinitions
	{
		[Given(@"There is a skill to monitor called '([^']*)' with queue id '([^']*)' and queue name '([^']*)' and activity '([^']*)'")]
		public void GivenThereIsASkillToMonitorCalled(string skill, int queueId, string queueName, string activity)
		{
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			DataMaker.Data().Apply(new ActivityConfigurable
			{
				Name = activity
			});

			DataMaker.Data().Apply(new SkillConfigurable
			{
				Name = skill,
				Activity = activity
			});
			
			DataMaker.Data().Analytics().Apply(new AQueue(datasourceData) { QueueId = queueId });

			DataMaker.Data().Apply(new QueueSourceConfigurable
			{
				Name = queueName,
				QueueId = queueId
			});

			DataMaker.Data().Apply(new WorkloadConfigurable
			{
				WorkloadName = skill,
				SkillName = skill,
				QueueSourceName = queueName,
				Open24Hours = true
			});
		}

		
		[Given(@"There is an email-like skill to monitor called '([^']*)' with queue id '([^']*)' and queue name '([^']*)' and activity '([^']*)'")]
		public void GivenThereIsAnEmailSkillToMonitorCalled(string skill, int queueId, string queueName, string activity)
		{
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			DataMaker.Data().Apply(new ActivityConfigurable
			{
				Name = activity
			});

			DataMaker.Data().Apply(new SkillConfigurable
			{
				Name = skill,
				Activity = activity,
				SkillType = "SkillTypeBackoffice",
				ShowAbandonRate = false,
				ShowReforecastedAgents = false
			});

			DataMaker.Data().Analytics().Apply(new AQueue(datasourceData) { QueueId = queueId });

			DataMaker.Data().Apply(new QueueSourceConfigurable
			{
				Name = queueName,
				QueueId = queueId
			});

			DataMaker.Data().Apply(new WorkloadConfigurable
			{
				WorkloadName = skill,
				SkillName = skill,
				QueueSourceName = queueName,
				Open24Hours = true
			});
		}

		[Given(@"there is queue statistics for the skill '(.*)' up until '(.*)'")]
		public void GivenThereIsQueueStatisticsForTheSkillUpUntil(string skillName, string time)
		{
			var latestStatisticsTime = DateTime.Parse(time);

			DataMaker.Data().Analytics().Apply(new QueueStatisticsForSkill(skillName, latestStatisticsTime));
		}

		[Given(@"there is forecast data for skill '(.*)' for date '(.*)'")]
		public void GivenThereIsForecastDataForSkillForDate(string skillName, string date)
		{
			var theDate = date == "today" ? DateTime.Now.Date : DateTime.Parse(date);

			DataMaker.Data().Apply(new ForecastConfigurable(skillName, theDate));
		}

		[Given(@"I select to create a new Skill Area")]
		public void GivenISelectToCreateANewSkillArea()
		{
			Browser.Interactions.Click(".skill-area-create");
		}

		[Given(@"I name the Skill Area '(.*)'")]
		public void GivenINameTheSkillArea(string skillAreaName)
		{
			Browser.Interactions.FillWith("#skillAreaName", skillAreaName);
		}

		[Given(@"I pick the skill '(.*)'")]
		[When(@"I pick the skill '(.*)'")]
		public void GivenIPickTheSkill(string skillName)
		{
			Browser.Interactions.AssertExists(".c3");
			var javascript = "var scope = angular.element(document.querySelector('.c3')).scope();" +
							 "var skillet = scope.skills.find(function(e){{return e.Name === '" + skillName + "'}});" +
							 "scope.selectedSkill = skillet;" +
							 "scope.selectedSkillChange(skillet);" +
							 "setTimeout(function(){console.log('delay')}, 1000);"; 
			Browser.Interactions.Javascript(javascript);
		}

		[Given(@"I select the skill '(.*)'")]
		[When(@"I select the skill '(.*)'")]
		public void GivenISelectTheSkill(string skillName)
		{
			Browser.Interactions.ClickContaining(".skill-area-list", skillName);
		}

		[When(@"I am done creating Skill Area")]
		public void WhenIAmDoneCreatingSkillArea()
		{
			Browser.Interactions.Click(".skill-area-save");
		}

		[Then(@"I select to monitor skill area '(.*)'")]
		public void ThenISelectToMonitorSkillArea(string skillArea)
		{
			Browser.Interactions.Javascript("document.querySelector(\"#skill-area-input\").focus();");
			var listId = "#" + Browser.Interactions.Javascript("return $('#skill-area-id input').attr(\"aria-owns\")");
			Browser.Interactions.Javascript($"$('{listId} li:contains(\"{skillArea}\")').click()");
		}

		[Then(@"I should no longer be able to monitor '(.*)'")]
		public void ThenIShouldNoLongerBeAbleToMonitor(string skillArea)
		{
			Browser.Interactions.Javascript("document.querySelector(\"#skill-area-input\").focus();");
			var listId = "#" + Browser.Interactions.Javascript("return $('#skill-area-id input').attr(\"aria-owns\")");
			Browser.Interactions.AssertFirstNotContains(listId, skillArea);
		}
		
		[Then(@"I should monitor '(.*)'")]
		public void ThenIShouldMonitor(string monitorItem)
		{
			Browser.Interactions.AssertAnyContains(".intraday-monitor-item", monitorItem);
		}

		[When(@"I select to remove '(.*)'")]
		public void WhenISelectToRemove(string skillArea)
		{
			Browser.Interactions.Click(".skill-area-options");
			Browser.Interactions.Click(".skill-area-delete");
			Browser.Interactions.Click(".skill-area-delete-confirm");
		}

		[Given(@"there is a Skill Area called '(.*)' that monitors skill '(.*)'")]
		public void GivenThereIsASkillAreaCalledThatMonitorsSkill(string skillArea, string skill)
		{
			DataMaker.Data().Apply(new SkillAreaConfigurable()
			{
				Name = skillArea,
				Skill = skill 
			});
		}

		[Given(@"there is a Skill Area called '(.*)' that monitors skills '(.*)'")]
		public void GivenThereIsASkillAreaCalledThatMonitorsSkills(string skillArea, string skills)
		{
			DataMaker.Data().Apply(new SkillAreaConfigurable()
			{
				Name = skillArea,
				Skills = skills
			});
		}

		[Then(@"I should see incoming traffic data in the chart")]
		[When(@"I should see incoming traffic data in the chart")]
		public void ThenIShouldSeeIncomingTrafficDataInTheChart()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var forecastedCalls = parseFloat(scope.viewObj.forecastedCallsObj.series[1]);" +
				"return (forecastedCalls >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var calls = parseFloat(scope.viewObj.actualCallsObj.series[1]);" +
				"return (calls >= 0);" 
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
							"var scope = angular.element(document.querySelector('.c3')).scope();" +
							"var faht = parseFloat(scope.viewObj.forecastedAverageHandleTimeObj.series[1]);" +
							"return (faht >= 0);"
							, "True");
			Browser.Interactions.AssertJavascriptResultContains(
							"var scope = angular.element(document.querySelector('.c3')).scope();" +
							"var aaht = parseFloat(scope.viewObj.actualAverageHandleTimeObj.series[1]);" +
							"return (aaht >= 0);"
							, "True");
		}

		[Given(@"I should see a summary of incoming traffic")]
		[Then(@"I should see a summary of incoming traffic")]
		public void ThenIShouldSeeASummaryOfIncomingTraffic()
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.forecasted-calls').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.calculated-calls').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.calls-difference').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.forecasted-aht').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.aht').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.aht-difference ').text().length > 0", "True");
		}

		[Given(@"There's no data available")]
		public void GivenIShouldNotSeeIncomingTrafficDataInTheChart()
		{
			Browser.Interactions.IsVisible(".no-data-available");
		}

		[When(@"I choose to monitor '(.*)'")]
		public void WhenIChooseToMonitor(string p0)
		{
			ScenarioContext.Current.Pending();
		}

		[When(@"I am navigating to intraday performance view")]
		public void WhenIAmNavigatingToIntradayPerformanceView()
		{
			Browser.Interactions.Javascript("$('md-tab-item:nth-child(2)').click();");
		}

		[Then(@"I should see performance data in the chart")]
		public void ThenIShouldSeePerformanceDataInTheChart()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var asa = parseFloat(scope.viewObj.averageSpeedOfAnswerObj.series[1]);" +
				"return (asa >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var abandonedRate = parseFloat(scope.viewObj.abandonedRateObj.series[1]);" +
				"return (abandonedRate >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var sl = parseFloat(scope.viewObj.serviceLevelObj.series[1]);" +
				"return (sl >= 0);"
				, "True");
			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			if (toggleQuerier.IsEnabled(Toggles.Wfm_Intraday_ESL_41827))
			{
				Browser.Interactions.AssertJavascriptResultContains(
					"var scope = angular.element(document.querySelector('.c3')).scope();" +
					"var esl = parseFloat(scope.viewObj.estimatedServiceLevelObj.series[1]);" +
					"return (esl >= 0) + ' |scopeViewObjSeries: ' + scope.viewObj.estimatedServiceLevelObj.series[1] + ' |esl: ' + esl;"
					, "true");
			}
		}

		[Then(@"I should see a summary of today's performance")]
		public void ThenIShouldSeeASummaryOfTodaySPerformance()
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.service-level').text().length > 0", "True");
			var toggleQuerier = new ToggleQuerier(TestSiteConfigurationSetup.URL.ToString());
			if (toggleQuerier.IsEnabled(Toggles.Wfm_Intraday_ESL_41827))
			{
				Browser.Interactions.AssertJavascriptResultContains("return $('.esl').text().length > 0", "True");
			}
			Browser.Interactions.AssertJavascriptResultContains("return $('.abandoned-rate').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.average-speed-of-answer').text().length > 0", "True");
		}

		[Given(@"there are scheduled agents for '(.*)' for date '(.*)'")]
		public void GivenThereAreScheduledAgentsForForDate(string skill, string date)
		{
			var theDate = date == "today" ? DateTime.Now.Date : DateTime.Parse(date);
			DataMaker.Data().Apply(new SkillCombinationResourceReadModelConfigurable(skill, theDate));
		}

		[When(@"I am navigating to intraday staffing view")]
		public void WhenIAmNavigatingToIntradayStaffingView()
		{
			Browser.Interactions.Javascript("setTimeout(function(){ $('md-tab-item:nth-child(3)').click(); }, 500);");
		}

		[Then(@"I should see staffing data in the chart")]
		public void ThenIShouldSeeStaffingDataInTheChart()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var forecastedStaffing = parseFloat(scope.viewObj.forecastedStaffing.series[1]);" +
				"return (forecastedStaffing >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var count = scope.viewObj.forecastedStaffing.updatedSeries.length;" +
				"var forecastedStaffing = parseFloat(scope.viewObj.forecastedStaffing.updatedSeries[count-1]);" +
				"return (forecastedStaffing >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var scheduledStaffing = parseFloat(scope.viewObj.scheduledStaffing[1]);" +
                "return (scheduledStaffing >= 0) + ' |sopeViewObjScheduledStaffing: ' + scope.viewObj.scheduledStaffing[1] + ' |scheduledStaffing: ' + scheduledStaffing;"
                , "true");
		}

		[Then(@"I should see forecasted staffing data in the chart")]
		public void ThenIShouldSeeForecastedStaffingDataInTheChart()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var forecastedStaffing = parseFloat(scope.viewObj.forecastedStaffing.series[1]);" +
				"return (forecastedStaffing >= 0);"
				, "True");
		}


		[When(@"I choose to look at statistics for '(.*)'")]
		public void WhenIChooseToLookAtStatisticsFor(string offset)
		{

			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see that the date is '(.*)'")]
		public void ThenIShouldSeeThatTheDateIs(string p0)
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see the date")]
		public void ThenIShouldSeeTheDate()
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('#viewingDate').text().length > 0", "True");
		}


		[When(@"I should not see incoming traffic data in the chart")]
		public void WhenIShouldNotSeeIncomingTrafficDataInTheChart()
		{
			Browser.Interactions.AssertExists(".no-data-available");
		}

		[When(@"I change date offset to '(.*)'")]
		public void WhenIChangeDateOffsetTo(int offset)
		{
			Browser.Interactions.Javascript("var scope = angular.element(document.querySelector('date-offset')).scope();" +
											$"scope.changeChosenOffset('{offset}');" +
											"setTimeout(function(){console.log('delay')}, 1000);");
		}

		[Then(@"I should see the export to excel button")]
		public void ThenIShouldSeeTheExportToExcelButton()
		{
			Browser.Interactions.AssertExists(".mdi-file-excel");
		}

		[Then(@"I should see the no abandonrate warning")]
		public void ThenIShouldSeeTheAbandon_RateWarning()
		{
			Browser.Interactions.AssertExists("#noAbandonRate");
		}
		[Then(@"I should see the no reforcasted warning")]
		public void ThenIShouldSeeTheNoReforcastedWarning()
		{
			Browser.Interactions.AssertExists("#noReforcastedAgents");
		}

		[Then(@"I should not se summary for abandonrate")]
		public void ThenIShouldNotSeSummaryForAbandonrate()
		{
			Browser.Interactions.AssertNotExists(".service-level", ".abandoned-rate");
		}

	}
}
