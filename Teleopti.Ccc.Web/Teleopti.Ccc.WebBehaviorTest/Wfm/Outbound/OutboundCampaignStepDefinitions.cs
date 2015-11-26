﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Outbound
{
	[Binding]
	public class OutboundCampaignStepDefinitions
	{

		[When(@"I set the starting month for viewing period to '(.*)'")]
		[Given(@"I set the starting month for viewing period to '(.*)'")]
		public void WhenISetTheStartingMonthForViewingPeriodTo(DateTime startingMonth)
		{
			Browser.Interactions.SetScopeValues(".outbound-summary", new Dictionary<string, string>
			{
				{"settings.periodStart" , string.Format("new Date('{0}')", startingMonth.ToShortDateString())} 
			});
		}

		[Then(@"I should see the gantt chart")]
		public void ThenIShouldSeeTheGanttChart()
		{
			Browser.Interactions.AssertExists(".outbound-gantt-chart");
		}

		[When(@"I can see '(.*)' in campaign list")]
		public void WhenICanSeeInCampaignList(string campaignName)
		{
			Browser.Interactions.AssertAnyContains(".outbound-gantt-chart", campaignName);
		}


		[Then(@"I should see '(.*)' in campaign list")]
		public void ThenIShouldSeeInCampaignList(string campaignName)
		{
			Browser.Interactions.AssertAnyContains(".outbound-gantt-chart", campaignName);
		}

		[Then(@"I should not see '(.*)' in campaign list")]
		public void ThenIShouldNotSeeInCampaignList(string campaignName)
		{
			Browser.Interactions.AssertNoContains(".outbound-gantt-chart", ".outbound-gantt-chart", campaignName);
		}

		[When(@"I click at campaign name tag '(.*)'")]
		public void WhenIClickAtCampaignNameTag(string campaignName)
		{
			Browser.Interactions.WaitScopeCondition(".outbound-summary", "isRefreshingGantt", Is.EqualTo("false"),
				() =>
				{			
					Browser.Interactions.ClickVisibleOnly(".campaign-visualization-toggle");
				});
		}
	
		[Then(@"I should see the backlog visualization of '(.*)'")]
		[When(@"I should see the backlog visualization of '(.*)'")]
		public void ThenIShouldSeeTheBacklogVisualizationOf(string campaignName)
		{
			Browser.Interactions.AssertExists("campaign-chart");
		}

		[Then(@"I should see the new campaign form")]
		[When(@"I see the new campaign form")]
		public void ThenIShouldSeeTheNewCampaignForm()
		{
			Browser.Interactions.AssertExists(".campaign-create");
		}

		[When(@"I submit the campaign form with the campaign detail")]
		public void WhenISubmitTheCampaignFormWithTheCampaignDetail(Table table)
		{			
			var instance = new OutboundCampaignConfigurable();
			table.FillInstance(instance);

			Browser.Interactions.SetScopeValues(".campaign-create", new Dictionary<string, string>
			{
				{ "campaign.Name" , string.Format("\"{0}\"", instance.Name)},
 				{ "campaign.Activity", string.Format("\"{0}\"", instance.Name) },
				{ "campaign.CallListLen", instance.CallListLen.ToString() },
				{ "campaign.TargetRate", instance.TargetRate.ToString() },
				{ "campaign.ConnectRate", instance.ConnectRate.ToString() },
				{ "campaign.RightPartyConnectRate", instance.RightPartyConnectRate.ToString() },
				{ "campaign.ConnectAverageHandlingTime", instance.ConnectAverageHandlingTime.ToString() },
				{ "campaign.RightPartyAverageHandlingTime", instance.RightPartyAverageHandlingTime.ToString() },
				{ "campaign.UnproductiveTime", instance.UnproductiveTime.ToString() },
				{ "campaign.StartDate.Date", string.Format("new Date('{0}')", instance.StartDate)},
				{ "campaign.EndDate.Date", string.Format("new Date('{0}')", instance.EndDate) },
				{ "campaign.WorkingHours", instance.GetWorkingHoursString()},
				{ "preventAutomaticRedirect", "true"}
			});

			Browser.Interactions.WaitScopeCondition(".campaign-create", "isInputValid()", Is.EqualTo("true"), () =>

					Browser.Interactions.ClickVisibleOnly(".form-submit.wfm-btn-primary"));							
		}

		[When(@"after the creation I goto the campaign list page")]
		public void WhenAfterTheCreationIGotoTheCampaignListPage()
		{
			Browser.Interactions.AssertScopeValue(".campaign-create", "campaign.Name", Is.Null.Or.Empty);
			Browser.Interactions.AssertScopeValue(".campaign-create", "isCreating", Is.EqualTo("false"));
			Navigation.GoToOutbound();
		}

		[When(@"I confirm to delete the campaign")]
		public void WhenIConfirmToDeleteTheCampaign()
		{
			Browser.Interactions.WaitScopeCondition(".campaign-edit", "isCampaignLoaded()", Is.EqualTo("true"), () =>
			{
				Browser.Interactions.ClickVisibleOnly(".trigger-campaign-delete");
				Browser.Interactions.ClickVisibleOnly(".modal-box .confirm-delete");
			});							
		}

		[When(@"after that I am redirected to the campaign list page")]
		public void WhenAfterThatIAmRedirectedToTheCampaignListPage()
		{
			Browser.Interactions.AssertExists(".outbound-gantt-chart");
		}

		[When(@"I see the edit campaign form")]
		public void WhenISeeTheEditCampaignForm()
		{
			Browser.Interactions.AssertExists(".campaign-edit");
		}

		[When(@"I change the campaign period to")]
		public void WhenIChangeTheCampaignPeriodTo(Table table)
		{
			var instance = new OutboundCampaignConfigurable();
			table.FillInstance(instance);

			Browser.Interactions.SetScopeValues(".campaign-edit", new Dictionary<string, string>
			{				
				{ "campaign.StartDate.Date", string.Format("new Date('{0}')", instance.StartDate)},
				{ "campaign.EndDate.Date", string.Format("new Date('{0}')", instance.EndDate) },
				{ "campaignSpanningPeriodForm.$pristine", "false" }
			});

			Browser.Interactions.Click(".form-submit.wfm-btn-primary");
		}

		[When(@"after the update is done I goto the campaign list page")]
		public void WhenAfterTheUpdateIsDoneIGotoTheCampaignListPage()
		{
			Navigation.GoToOutbound();			
		}

		[Given(@"I have created a campaign with")]
		public void GivenIHaveCreatedACampaignWith(Table table)
		{
			DataMaker.ApplyFromTable<OutboundCampaignConfigurable>(table);
		}

		[Given(@"I am viewing outbounds page")]
		public void GivenIAmViewingOutPage()
		{
			TestControllerMethods.Logon();
			Navigation.GoToOutbound();
		}


		[When(@"I have created campaign with")]
		public void WhenIHaveCreatedCampaignWith(Table table)
		{			
			TestControllerMethods.Logon();
			Navigation.GoToOutboundCampaignCreation();
			ThenIShouldSeeTheNewCampaignForm();
			WhenISubmitTheCampaignFormWithTheCampaignDetail(table);
			WhenAfterTheCreationIGotoTheCampaignListPage();
		}

		[When(@"I view the backlog chart of the campaign created with")]
		public void WhenIViewTheBacklogChartOfTheCampaignCreatedWith(Table table)
		{
			var instance = new OutboundCampaignConfigurable();
			table.FillInstance(instance);
			WhenIHaveCreatedCampaignWith(table);
			WhenISetTheStartingMonthForViewingPeriodTo(instance.StartDate);
			WhenIClickAtCampaignNameTag(instance.Name);
			Browser.Interactions.AssertExists("campaign-chart");
		}

		[When(@"I view the detail of the campaign created with")]
		public void WhenIViewTheDetailOfTheCampaignCreatedWith(Table table)
		{
			WhenIViewTheBacklogChartOfTheCampaignCreatedWith(table);
			Browser.Interactions.ClickVisibleOnly(".btn-goto-edit-campaign");
		}

		[When(@"I select all the dates from '(.*)' to '(.*)'")]
		public void WhenISelectAllTheDatesFromTo(string startDate, string endDate)
		{
			var dateTimePeriod = new DateTimePeriod(DateTime.SpecifyKind(DateTime.Parse(startDate), DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.Parse(endDate), DateTimeKind.Utc));

			var dates = dateTimePeriod.ToDateOnlyPeriod(TimeZoneInfo.Local).DayCollection().Select(d => "'" + d.ToShortDateString() + "'");
			var datesString = string.Join(", ", dates);

			Browser.Interactions.SetScopeValues("div[id^=Chart]", new Dictionary<string, string>
			{
				{"campaign.selectedDates" , "[" + datesString + "]" } 
			});
		}

		[When(@"I set the manual production plan to '(.*)'")]
		public void WhenISetTheManualProductionPlanTo(int planValue)
		{
			Browser.Interactions.SetScopeValues("campaign-commands-pane", new Dictionary<string, string>
			{
				{"manualPlanSwitch", "true"},
				{"manualPlanInput" , planValue.ToString() } 
			}, true);

			Browser.Interactions.WaitScopeCondition("campaign-commands-pane", "validManualProductionPlan()", Is.EqualTo("true"),
				() => {
					Browser.Interactions.ClickVisibleOnly(".btn-save-plan");
				}, true);
		}


		[Then(@"I should see all planned person hours to be '(.*)'")]
		public void ThenIShouldSeeAllPlannedPersonHoursToBe(int planValue)
		{
			var check = string.Format("function(x) {{ return !angular.isNumber(x) ||  x == {0}; }}", planValue);
			var target = string.Format("campaign.graphData.unscheduledPlans.every({0})", check);
			Browser.Interactions.AssertScopeValue("div[id^=Chart]", target, Is.EqualTo("true"));
		}

		[When(@"I see that the campaign is not done after the end date")]
		public void WhenISeeThatTheCampaignIsNotDoneAfterTheEndDate()
		{
			var target = string.Format("campaign.graphData.rawBacklogs.slice(-1)[0] > {0}", getZeroValidationTolerance());
			Browser.Interactions.AssertScopeValue("div[id^=Chart]", target, Is.EqualTo("true"));			
		}

		[When(@"I replan the campaign")]
		public void WhenIReplanTheCampaign()
		{
			Browser.Interactions.ClickVisibleOnly(".btn-replan");			
		}

		[Then(@"I should see the campaign is done after the end date")]
		public void ThenIShouldSeeTheCampaignIsDoneAfterTheEndDate()
		{
			var target = string.Format("campaign.graphData.rawBacklogs.slice(-1)[0] < {0}", getZeroValidationTolerance());
			Browser.Interactions.AssertScopeValue("div[id^=Chart]", target, Is.EqualTo("true"));
		}

		[When(@"I set the manual backlog to '(.*)'")]
		public void WhenISetTheManualBacklogTo(int backlogValue)
		{
			Browser.Interactions.SetScopeValues("campaign-commands-pane", new Dictionary<string, string>
			{
				{"manualBacklogSwitch", "true"},
				{"manualBacklogInput" , backlogValue.ToString() } 
			}, true);

			Browser.Interactions.WaitScopeCondition("campaign-commands-pane", "validManualBacklog()", Is.EqualTo("true"),
				() =>
				{
					Browser.Interactions.ClickVisibleOnly(".btn-save-backlog");
				}, true);
		}

		[Then(@"the campaign is overstaffed")]
		public void ThenTheCampaignIsOverstaffed()
		{
			Browser.Interactions.AssertScopeValue("div[id^=Chart]", "campaign.WarningInfo[0].TypeOfRule", Is.EqualTo("campaignoverstaff"));
		}



		private int getZeroValidationTolerance()
		{
			return 5;
		}

	}
}
