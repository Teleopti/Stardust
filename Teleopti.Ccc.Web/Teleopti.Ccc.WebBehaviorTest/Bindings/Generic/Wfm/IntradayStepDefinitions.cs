﻿using System;
using System.Threading;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class IntradayStepDefinitions
	{
		[Given(
			@"There is a skill to monitor called '([^']*)' with queue id '([^']*)' and queue name '([^']*)' and activity '([^']*)'")]
		public void GivenThereIsASkillToMonitorCalled(string skill, int queueId, string queueName, string activity)
		{
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			DataMaker.Data().Apply(new ActivitySpec
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


		[Given(
			@"There is an email-like skill to monitor called '([^']*)' with queue id '([^']*)' and queue name '([^']*)' and activity '([^']*)'")]
		public void GivenThereIsAnEmailSkillToMonitorCalled(string skill, int queueId, string queueName, string activity)
		{
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			DataMaker.Data().Apply(new ActivitySpec
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

		[Given(@"Local storage is reset")]
		public void GivenLocalStorageIsReset()
		{
			var javascript = "window.localStorage.clear();";

			Browser.Interactions.Javascript(javascript);
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

		[Given(@"there is forecast data for skill '(.*)' opened whole day for next two weeks")]
		public void GivenThereIsForecastDataForSkillOpenedWholeDayForNextTwoWeeks(string skillName)
		{
			var startDate = DateTime.Now.Date;
			var endDate = startDate.AddDays(13);
			for (var date = startDate; date.CompareTo(endDate) <= 0; date = date.AddDays(1))
			{
				DataMaker.Data().Apply(new ForecastConfigurable(skillName, date, true));
			}
		}

		[Given(@"I select to create a new Skill Area")]
		public void GivenISelectToCreateANewSkillArea()
		{
			Browser.Interactions.Click(".skill-area-create");
		}

		[Given(@"I select to create a new Skill Group")]
		public void GivenISelectToCreateANewSkillGroup()
		{
			Browser.Interactions.Click("#manage_skill_group_button");
		}

		[Given(@"I select to create a new Skill Group in SGM")]
		[When(@"I select to create a new Skill Group in SGM")]
		public void GivenISelectToCreateANewSkillGroupInSGM()
		{
			Browser.Interactions.AssertExists("#create-skill-group-button");
			Browser.Interactions.Click("#create-skill-group-button");
		}


		[Given(@"I name the Skill Area '(.*)'")]
		public void GivenINameTheSkillArea(string skillAreaName)
		{
			Browser.Interactions.FillWith("#skillAreaName", skillAreaName);
		}

		[Given(@"I name the Skill Group '(.*)'")]
		[When(@"I name the Skill Group '(.*)'")]
		public void GivenINameTheSkillGroup(string skillGroupName)
		{
			Browser.Interactions.TryUntil(
				() => { Browser.Interactions.FillWith("#groupNameBox", skillGroupName); },
				() => Browser.Interactions.IsVisible("#confirmEditNameButton"),
				TimeSpan.FromMilliseconds(1000));
			Browser.Interactions.Click("#confirmEditNameButton");
		}

		[Given(@"I select the Skill Group '(.*)'")]
		public void GivenISelectTheSkillGroup(string skillGroupName)
		{
			Browser.Interactions.TryUntil(
				() => { Browser.Interactions.ClickContaining("#available_skill_groups_list", skillGroupName); },
				() => Browser.Interactions.IsVisible("#available_skill_groups_list"),
				TimeSpan.FromSeconds(1));
		}

		[Given(@"I chose to rename the Skill Group '(.*)'")]
		public void GivenIChoseToRenameTheSkillGroup(string skillGroupName)
		{
			Browser.Interactions.AssertExists("form");
			var javascript = "var scope = angular.element(document.querySelector('form')).scope();" +
							 "var group = scope.vm.skillGroups.find(function(e){{return e.Name === '" + skillGroupName + "'}});" +
							 "scope.vm.selectedSkillGroup = group;" +
							 "scope.vm.editNameClicked(group);";

			Browser.Interactions.Javascript(javascript);
		}


		[Given(@"I select to manage Skill Groups")]
		[When(@"I select to manage Skill Groups")]
		[Then(@"I select to manage Skill Groups")]
		public void GivenISelectToManageSkillGroups()
		{
			Browser.Interactions.Click("#manage_skill_group_button");
		}

		[Given(@"I pick the skill '(.*)'")]
		[When(@"I pick the skill '(.*)'")]
		public void GivenIPickTheSkill(string skillName)
		{
			Browser.Interactions.AssertExists("skill-picker");
			var javascript = "var scope = angular.element(document.querySelector('skill-picker')).scope();" +
							 "var skillet = scope.vm.skills.find(function(e){{return e.Name === '" + skillName + "'}});" +
							 "scope.vm.selectedSkill = skillet;" +
							 "scope.vm.selectSkillOrSkillArea(skillet);";

			Browser.Interactions.Javascript(javascript);
		}

		[Given(@"I pick the skillgroup '(.*)'")]
		[When(@"I pick the skillgroup '(.*)'")]
		public void GivenIPickTheSkillGroup(string skillName)
		{
			//Browser.SetDefaultTimeouts(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
			Browser.Interactions.AssertExists("the-skill-picker");

			Browser.Interactions.ClickVisibleOnly("the-skill-picker .con-flex:nth-child(2n) div.wfm-form input");
			Browser.Interactions.ClickContaining("the-skill-picker .con-flex:nth-child(2n) div.wfm-form .wfm-dropdown-panel li", skillName);
		}

		[Given(@"I select the skill '(.*)'")]
		[When(@"I select the skill '(.*)'")]
		public void GivenISelectTheSkill(string skillName)
		{
			Browser.Interactions.ClickContaining(".skill-area-list", skillName);
		}

		[Given(@"I select the skill '(.*)' in SGM")]
		[When(@"I select the skill '(.*)' in SGM")]
		public void GivenISelectTheSkillInSGM(string skillName)
		{
			Browser.Interactions.ClickContaining("#available_skills_list", skillName);
		}

		[When(@"I am done creating Skill Area")]
		public void WhenIAmDoneCreatingSkillArea()
		{
			Browser.Interactions.Click(".skill-area-save");
		}

		[When(@"I save the Skill Groups")]
		[Given(@"I save the Skill Groups")]
		public void WhenISaveTheSkillGroups()
		{
			Browser.Interactions.Click("#save_all");
		}

		[When(@"I close the Skill Manager")]
		[Then(@"I close the Skill Manager")]
		public void WhenICloseTheSkillManager()
		{
			try
			{
				var n = 10;
				while (Browser.Interactions.IsVisible("#exit_sgm") && !Browser.Interactions.IsVisible("#confirmExitButton"))
				{
					Browser.Interactions.Click("#exit_sgm");
					n--;
					if (n == 0)
					{
						break;
					}
				}
				if (Browser.Interactions.IsVisible("#confirmExitButton"))
					Browser.Interactions.Click("#confirmExitButton");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		[Then(@"I should see '(.*)' as included skill")]
		public void ThenIShouldSeeAsIncludedSkill(string skill)
		{
			Browser.Interactions.AssertAnyContains(".chip-text", skill);
		}

		[Given(@"I select '(.*)' from SkillsInThisGroup")]
		public void GivenISelectFromSkillsInThisGroup(string skill)
		{
			var elementSelector = $"$(\"span:contains('{skill}')\").parent(\"div.wfm-chip-wrap\")";

			Browser.Interactions.Javascript($"{elementSelector}.click();");
		}

		[Then(@"I should not see '(.*)' as included skill")]
		public void ThenIShouldNotSeeAsIncludedSkill(string skill)
		{
			Browser.Interactions.AssertFirstNotContains(".chip-text", skill);
		}

		[Given(@"I select '(.*)' from available skill")]
		public void GivenISelectFromAvailableSkill(string skill)
		{
			var elementSelector = $"$(\"span:contains('{skill}')\").parent()";
			Browser.Interactions.Javascript($"{elementSelector}.click();");
		}

		[Given(@"I select to monitor skill area '(.*)'")]
		[Then(@"I select to monitor skill area '(.*)'")]
		public void ThenISelectToMonitorSkillArea(string skillArea)
		{
			Thread.Sleep(1000);
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
			DataMaker.Data().Apply(new SkillGroupConfigurable
			{
				Name = skillArea,
				Skill = skill
			});
		}

		[Given(@"there is a Skill Area called '(.*)' that monitors skills '(.*)'")]
		public void GivenThereIsASkillAreaCalledThatMonitorsSkills(string skillArea, string skills)
		{
			DataMaker.Data().Apply(new SkillGroupConfigurable
			{
				Name = skillArea,
				Skills = skills
			});
		}

		[Then(@"I should see incoming traffic data in the chart")]
		[Given(@"I should see incoming traffic data in the chart")]
		[When(@"I should see incoming traffic data in the chart")]
		public void ThenIShouldSeeIncomingTrafficDataInTheChart()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var forecastedCalls = parseFloat(scope.vm.viewObj.forecastedCallsObj.series[1]);" +
				"return (forecastedCalls >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var calls = parseFloat(scope.vm.viewObj.actualCallsObj.series[1]);" +
				"return (calls >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var faht = parseFloat(scope.vm.viewObj.forecastedAverageHandleTimeObj.series[1]);" +
				"return (faht >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var aaht = parseFloat(scope.vm.viewObj.actualAverageHandleTimeObj.series[1]);" +
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
				"var asa = parseFloat(scope.vm.viewObj.averageSpeedOfAnswerObj.series[1]);" +
				"return (asa >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var abandonedRate = parseFloat(scope.vm.viewObj.abandonedRateObj.series[1]);" +
				"return (abandonedRate >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var sl = parseFloat(scope.vm.viewObj.serviceLevelObj.series[1]);" +
				"return (sl >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var esl = parseFloat(scope.vm.viewObj.estimatedServiceLevelObj.series[1]);" +
				"return (esl >= 0) + ' |scopeViewObjSeries: ' + scope.vm.viewObj.estimatedServiceLevelObj.series[1] + ' |esl: ' + esl;"
				, "true");
		}

		[Then(@"I should see a summary of today's performance")]
		public void ThenIShouldSeeASummaryOfTodaySPerformance()
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.service-level').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.esl').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.abandoned-rate').text().length > 0", "True");
			Browser.Interactions.AssertJavascriptResultContains("return $('.average-speed-of-answer').text().length > 0",
				"True");
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
				"var forecastedStaffing = parseFloat(scope.vm.viewObj.forecastedStaffing.series[1]);" +
				"return (forecastedStaffing >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var count = scope.vm.viewObj.forecastedStaffing.updatedSeries.length;" +
				"var forecastedStaffing = parseFloat(scope.vm.viewObj.forecastedStaffing.updatedSeries[count-1]);" +
				"return (forecastedStaffing >= 0);"
				, "True");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var scheduledStaffing = parseFloat(scope.vm.viewObj.scheduledStaffing[1]);" +
				"return (scheduledStaffing >= 0) + ' |sopeViewObjScheduledStaffing: ' + scope.vm.viewObj.scheduledStaffing[1] + ' |scheduledStaffing: ' + scheduledStaffing;"
				, "true");
		}

		[Then(@"I should see forecasted staffing data in the chart")]
		[When(@"I should see forecasted staffing data in the chart")]
		public void ThenIShouldSeeForecastedStaffingDataInTheChart()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('.c3')).scope();" +
				"var forecastedStaffing = parseFloat(scope.vm.viewObj.forecastedStaffing.series[1]);" +
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

		[Given(@"I change date offset to '(.*)'")]
		[When(@"I change date offset to '(.*)'")]
		public void WhenIChangeDateOffsetTo(int offset)
		{
			Browser.Interactions.Javascript("var scope = angular.element(document.querySelector('date-offset')).scope();" +
											$"scope.vm.changeChosenOffset('{offset}');" +
											"setTimeout(function(){console.log('delay')}, 1000);");
		}

		[Then(@"I should see the offset is set to '(.*)'")]
		public void ShouldSeeOffsetIsSetTo(int offset)
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#skill-id')).scope();" +
				$"scope.vm.chosenOffset === '{offset}';", "True");
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

		[When(@"I select skill '(.*)' from included skills in skill group")]
		[Given(@"I select skill '(.*)' from included skills in skill group")]
		public void GivenISelectSkillFromIncludedSkillsInSkillGroup(string skill)
		{
			var elementSelector = $"$(\"span:contains('{skill}')\").parent(\"span.wfm-chip\")";
			Browser.Interactions.AssertJavascriptResultContains($"return {elementSelector}[0] !== undefined", "True");
			Browser.Interactions.Javascript($"{elementSelector}.click();");
		}

		[Then(@"I Should see skill '(.*)' as selected skill")]
		public void ThenIShouldSeeSkillAsSelectedSkill(string skill)
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#skill-id')).scope();" +
				$"return scope.vm.selectedItem.Name === '{skill}';", "True");
		}

		[Then(@"I Should not see any skill group selected")]
		public void ThenIShouldNotSeeAnySkillGroupSelected()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#skill-id')).scope();" +
				$"return scope.vm.selectedSkillArea === null;", "True");
		}

		[When(@"I return to skill group from skill '(.*)'")]
		public void WhenIReturnToSkillGroupFromSkill(string skill)
		{
			var elementSelector = $"$(\"span:contains('{skill}')\").parent(\"span.wfm-chip\")";
			Browser.Interactions.AssertJavascriptResultContains($"return {elementSelector}[0] !== undefined", "True");
			Browser.Interactions.Javascript($"{elementSelector}.click();");
		}


		[When(@"I return to skill group '(.*)'")]
		public void WhenIReturnToSkillGroup(string skillGroup)
		{
			Browser.Interactions.Click($"i[aria-label='{skillGroup}']");
		}

		[When(@"I choose to not monitor any skill or skillgroup")]
		public void WhenIChooseToNotMonitorAnySkillOrSkillgroup()
		{
			Browser.Interactions.Click($"button[aria-label='Clear Input']");
		}

		[Then(@"I should not see the chart")]
		public void ThenIShouldNotSeeTheChart()
		{
			Browser.Interactions.AssertExists("#chartPanel");
			Assert.IsFalse(Browser.Interactions.IsVisible("#chartPanel"), "Chart panel should not be visible");
		}
	}
}