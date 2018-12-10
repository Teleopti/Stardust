using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;


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
			Thread.Sleep(300);
			Browser.Interactions.AssertAnyContains(".outbound-gantt-chart", campaignName);
		}

		[Then(@"I should not see '(.*)' in campaign list")]
		public void ThenIShouldNotSeeInCampaignList(string campaignName)
		{
			Thread.Sleep(300);
			Browser.Interactions.AssertNoContains(".outbound-gantt-chart", ".outbound-gantt-chart", campaignName);
		}

		[When(@"I click at campaign name tag '(.*)'")]
		public void WhenIClickAtCampaignNameTag(string campaignName)
		{
			Browser.Interactions.AssertScopeValue(".outbound-summary", "isRefreshingGantt", false);
			Browser.Interactions.ClickVisibleOnly(".campaign-visualization-toggle");
		}
	
		[Then(@"I should see the backlog visualization of '(.*)'")]
		[When(@"I should see the backlog visualization of '(.*)'")]
		public void ThenIShouldSeeTheBacklogVisualizationOf(string campaignName)
		{
			Thread.Sleep(300);
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
				{ "campaign.SpanningPeriod", "{" + string.Format("startDate: new Date('{0}'), endDate: new Date('{1}')", instance.StartDate, instance.EndDate) + "}" },

				{ "campaign.StartDate", string.Format("new Date('{0}')", instance.StartDate)},
				{ "campaign.EndDate", string.Format("new Date('{0}')", instance.EndDate) },
				{ "campaign.WorkingHours", instance.GetWorkingHoursString()},
				{ "preventAutomaticRedirect", "true"}
			});

			Browser.Interactions.AssertScopeValue(".campaign-create", "isInputValid()", true);
			Browser.Interactions.ClickVisibleOnly(".campaign-create-submit");						
		}

		[When(@"after the creation I will redirect to the campaign list page")]
		public void WhenAfterTheCreationIRedirectToTheCampaignListPage()
		{
			Browser.Interactions.AssertExists(".outbound-gantt-chart");
		}		

		[When(@"I confirm to delete the campaign")]
		public void WhenIConfirmToDeleteTheCampaign()
		{
			Browser.Interactions.AssertScopeValue(".test-campaign-edit", "isCampaignLoaded()", true);
			Browser.Interactions.SetScopeValues(".test-campaign-edit", new Dictionary<string, string>
			{
				{"directRemoveCampaign", "true"}
			});
			Browser.Interactions.Click("form.ng-valid .test-delete-campaign");				
		}

		[When(@"after that I am redirected to the campaign list page")]
		public void WhenAfterThatIAmRedirectedToTheCampaignListPage()
		{
			Browser.Interactions.AssertExists(".outbound-gantt-chart");
		}

		[When(@"I see the edit campaign form")]
		public void WhenISeeTheEditCampaignForm()
		{
			Browser.Interactions.AssertExists(".test-campaign-edit form.ng-valid");
		}

		[When(@"I change the campaign period to")]
		public void WhenIChangeTheCampaignPeriodTo(Table table)
		{
			Browser.Interactions.AssertScopeValue(".test-campaign-edit", "isCampaignLoaded()", true);

			var instance = new OutboundCampaignConfigurable();
			table.FillInstance(instance);

			Browser.Interactions.SetScopeValues(".test-campaign-edit", new Dictionary<string, string>
			{				
				{ "campaign.SpanningPeriod", "{" + string.Format("startDate: new Date('{0}'), endDate: new Date('{1}')", instance.StartDate, instance.EndDate) + "}" }
			});
			Browser.Interactions.Click("form.ng-valid .test-campaign-edit-submit");
		}

		[When(@"after the update is done I will redirect to the campaign list page")]
		public void WhenAfterTheUpdateIsDoneIWillRedirectToTheCampaignListPage()
		{
			Browser.Interactions.AssertExists(".outbound-gantt-chart");
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
			WhenAfterTheCreationIRedirectToTheCampaignListPage();
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

			Browser.Interactions.WaitScopeCondition("campaign-commands-pane", "validManualProductionPlan()", true,
				() => {
					Browser.Interactions.ClickVisibleOnly(".btn-save-plan");
				}, true);
		}


		[Then(@"I should see all planned person hours to be '(.*)'")]
		public void ThenIShouldSeeAllPlannedPersonHoursToBe(int planValue)
		{
			var check = string.Format("function(x) {{ return !angular.isNumber(x) ||  x == {0}; }}", planValue);
			var target = string.Format("campaign.graphData.unscheduledPlans.every({0})", check);
			Browser.Interactions.AssertScopeValue("div[id^=Chart]", target, true);
		}

		[When(@"I see that the campaign is not done after the end date")]
		public void WhenISeeThatTheCampaignIsNotDoneAfterTheEndDate()
		{
			var target = string.Format("campaign.graphData.rawBacklogs.slice(-1)[0] > {0}", getZeroValidationTolerance());
			Browser.Interactions.AssertScopeValue("div[id^=Chart]", target, true);			
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
			Browser.Interactions.AssertScopeValue("div[id^=Chart]", target, true);
		}

		[When(@"I set the manual backlog to zero")]
		public void WhenISetTheManualBacklogTo()
		{
			Browser.Interactions.SetScopeValues("campaign-commands-pane", new Dictionary<string, string>
			{
				{"manualBacklogSwitch", "true"},
				{"manualBacklogInput" , "0" } 
			}, true);

			Thread.Sleep(100);

			Browser.Interactions.AssertScopeValue("campaign-commands-pane", "manualBacklogInput", "0", true);
			Browser.Interactions.Click(".btn-save-backlog");			
		}

		[Then(@"the campaign is overstaffed")]
		public void ThenTheCampaignIsOverstaffed()
		{
			Browser.Interactions.AssertScopeValue("div[id^=Chart]", "campaign.WarningInfo[0].TypeOfRule","CampaignOverstaff");
		}


		[Then(@"I should see the campaign start date to be '(.*)'")]
		public void ThenIShouldSeeTheCampaignStartDateToBe(int date)
		{
			Browser.Interactions.AssertAnyContains(".date-range-start-date .btn-info.active", date.ToString());
		}

		[Then(@"I should see the campaign end date to be '(.*)'")]
		public void ThenIShouldSeeTheCampaignEndDateToBe(int date)
		{
			Browser.Interactions.AssertAnyContains(".date-range-end-date .btn-info.active", date.ToString());
		}

		private int getZeroValidationTolerance()
		{
			return 5;
		}

	}
}
