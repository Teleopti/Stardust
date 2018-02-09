﻿using System;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Staffing
{
	[Binding]
	public class StaffingStepDefintions
	{
		[Given(@"there is a skill to monitor called '(.*)' with activity '(.*')")]
		public void GivenThereIsASkillToMonitorCalled(string skillName, string activity)
		{
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			DataMaker.Data().Apply(new ActivitySpec
			{
				Name = activity
			});

			DataMaker.Data().Apply(new SkillConfigurable
			{
				Name = skillName,
				Activity = activity
			});
		}

		[Given(@"there is a skill with name '(.*)', queue id '([^']*)', queue name '([^']*)' and activity '(.*')")]
		public void GivenThereIsASkillToMonitorWithName(string skillName, int queueId, string queueName, string activity)
		{
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			DataMaker.Data().Apply(new ActivitySpec
			{
				Name = activity
			});

			DataMaker.Data().Apply(new SkillConfigurable
			{
				Name = skillName,
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
				WorkloadName = skillName,
				SkillName = skillName,
				QueueSourceName = queueName,
				Open24Hours = true
			});
		}

		[Given(@"there is a skill group with name '(.*)' with skills '(.*)'")]
		public void GivenThereIsASkillGroupWithName(string name, string skills)
		{
			DataMaker.Data().Apply(new SkillGroupConfigurable
			{
				Name = name,
				Skills = skills
			});
		}

		[Given(@"there is a skill with name '(.*)' with activity '(.*)'")]
		public void GivenThereIsASkillWithName(string skillName, string activity)
		{
			var datasourceData = DefaultAnalyticsDataCreator.GetDataSources();

			DataMaker.Data().Apply(new ActivitySpec
			{
				Name = activity
			});

			DataMaker.Data().Apply(new SkillConfigurable
			{
				Name = skillName,
				Activity = activity
			});
		}

		[Given(@"there is queue statistics for skill '(.*)' until '(.*)'")]
		public void GivenThereIsQueueStatisticsForSkillUntill(string skillName, string time)
		{
			var latestStatisticsTime = DateTime.Parse(time);

			DataMaker.Data().Analytics().Apply(new QueueStatisticsForSkill(skillName, latestStatisticsTime));
		}

		[Given(@"there is staffing data for skills '(.*)' for date '(.*)'")]
		public void GivenThereIsForecastDataForSkillForDate(string skillName, string date)
		{
			var theDate = date == "today" ? DateTime.Now.Date : DateTime.Parse(date);

			DataMaker.Data().Apply(new ForecastConfigurable(skillName, theDate));
		}

		[Given(@"There is a compensation with name '(.*)'")]
		public void GivenThereIsACompensationWithName(string name)
		{
			DataMaker.Data().Apply(new MultiplicatorDefinitionSetConfigurable { Name = name });
		}


		[When(@"I select skill group '(.*)'")]
		public void WhenISelectSkillGroup(string group)
		{
			/*
			Browser.Interactions.AssertExists("md-autocomplete");
			var javascript = "var scope = angular.element(document.querySelector('md-autocomplete')).scope();" +
							 "var sg = scope.vm.skillAreas.find(function(e){{return e.Name === '" + group + "'}});" +
							 "scope.vm.selectedArea = sg;" +
							 "scope.vm.selectedAreaChange(sg);";
			Browser.Interactions.TryUntil(
				() => { Browser.Interactions.Javascript(javascript); },
				() => Browser.Interactions.IsVisible("md-autocomplete")
				, TimeSpan.FromSeconds(1)
			);*/

			
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#area-id')).scope();" +
				"var skillGroup = scope.vm.querySearchAreas()[0].Name;" +
				$"return (skillGroup === '{group}')", "True");


			Browser.Interactions.Javascript(
				"var scope = angular.element(document.querySelector('#area-id')).scope();" +
				"var sg = scope.vm.allSkillAreas.find(function(e){{return e.Name === '" + group + "'}});" +
				$"scope.vm.selectedSkillArea = sg;" +
				$"scope.vm.selectedAreaChange(sg);"
			);

			Browser.Interactions.PressEnter("#area-id");
			
		}

		[When(@"I change staffing date to '(.*)'")]
		public void WhenIChangeStaffingDateTo(string date)
		{
			Browser.Interactions.FillWith(
				"#teamschedule-datepicker-input",
				date
			);
			Thread.Sleep(600);
			Browser.Interactions.PressEnter("#teamschedule-datepicker-input");
		}

		[When(@"I press the create skill button")]
		public void WhenIPressCreateSkillButton()
		{
			Browser.Interactions.Click("#manage_skill_group_button");
		}

		[When(@"I press the BPO exchange button")]
		public void WhenIPressBpoExchangeButton()
		{
			Browser.Interactions.Click("#bpoExchangeBtn");
		}

		[When(@"I press the get suggestions for overtime button")]
		public void WhenIPressOvertimeSuggestionButton()
		{
			Browser.Interactions.Click("#overstaffBtn.wfm-btn-invis-primary");
		}


		[When(@"Using shrinkage is on")]
		public void WhenUsingShringageIsOn()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('md-checkbox')).scope();" +
				"var isUsingShrinkage = scope.vm.useShrinkage;" +
				"return isUsingShrinkage", "True"
			);
		}

		[When(@"Using shrinkage is off")]
		public void WhenUsingShringageIsOff()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('md-checkbox')).scope();" +
				"var isUsingShrinkage = scope.vm.useShrinkage;" +
				"return isUsingShrinkage", "False"
			);
		}


		[When(@"I turn using shrinkage to on")]
		public void WhenTogglingUsingShrinkageToOn()
		{
			Browser.Interactions.Click("md-checkbox");
			Thread.Sleep(400);
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('md-checkbox')).scope();" +
				"var isUsingShrinkage = scope.vm.useShrinkage;" +
				"return isUsingShrinkage", "True"
			);
		}


		[When(@"I turn using shrinkage to off")]
		public void WhenTogglingUsingShrinkageToOff()
		{
			Browser.Interactions.Click("md-checkbox");
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('md-checkbox')).scope();" +
				"var isUsingShrinkage = scope.vm.useShrinkage;" +
				"return isUsingShrinkage", "False"
			);
		}


		[Then(@"I should see the selected skill group '(.*)'")]
		public void ThenIShouldSeeTheSelectedSkillGroup(string group)
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#area-id')).scope();" +
				"var skillGroup = scope.vm.selectedArea;" +
				$"return (skillGroup.Name === '{group}')", "True");
		}


		[When(@"I see staffing data in the graph")]
		[Then(@"I should see staffing data in the graph")]
		public void ThenIShouldSeeStaffingDataInTheGraph()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#staffingChart')).scope();" +
				"var chartHasData = scope.vm.staffingDataAvailable;" +
				"return chartHasData", "True"
			);
		}


		[Then(@"Using shrinkage should be used")]
		public void ThenUsingShrinkageShouldBeUsed()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#staffingChart')).scope();" +
				"var isUsingShrinkage = scope.vm.useShrinkage;" +
				"return isUsingShrinkage", "True"
			);
		}


		[Then(@"I should see the staffing chart update")]
		public void ThenIShouldSeeStaffingChartUpdate()
		{
			// TODO: how to check for this?
		}


		[Then(@"I should see the manage skill group page")]
		public void ThenIShouldSeeManageSkillGroupPage()
		{
			var urlString = "skill-area-config";
			Browser.Interactions.AssertUrlContains(urlString);
		}


		[Then(@"I should see the bpo exchange page")]
		public void ThenIShouldSeeBpoExchangePage()
		{
			var urlString = "bpo";
			Browser.Interactions.AssertUrlContains(urlString);
		}


		[When(@"I can see overtime settings")]
		[Then(@"I should see overtime settings")]
		public void ThenIShouldSeeOvertimeSettings()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#staffingChart')).scope();" +
				"var showOvertime = scope.vm.showOverstaffSettings;" +
				"return showOvertime", "True"
			);
		}

		[When(@"I can not see overtime settings")]
		[Then(@"I should not see overtime settings")]
		public void ThenIShouldNotSeeOvertimeSettings()
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"var scope = angular.element(document.querySelector('#staffingChart')).scope();" +
				"var showOvertime = scope.vm.showOverstaffSettings;" +
				"return showOvertime", "False"
			);
		}
	}
}
