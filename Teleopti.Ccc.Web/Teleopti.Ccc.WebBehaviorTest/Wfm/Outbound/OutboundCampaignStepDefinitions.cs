using System;
using System.Collections.Generic;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Outbound
{
	[Binding]
	public class OutboundCampaignStepDefinitions
	{

		[When(@"I set the starting month for viewing period to '(.*)'")]
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
			Browser.Interactions.WaitScopeCondition(".outbound-summary", "isRefreshingGantt", "false",
				() => { Browser.Interactions.ClickContaining(".campaign-visualization-toggle", campaignName); });			
		}
	
		[Then(@"I should see the backlog visualization of '(.*)'")]
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
		
			Browser.Interactions.WaitScopeCondition(".campaign-create", "isInputValid()", "true", () => 
					Browser.Interactions.Click(".form-submit"));							
		}

		[When(@"after the creation I goto the campaign list page")]
		public void WhenAfterTheCreationIGotoTheCampaignListPage()
		{
			Browser.Interactions.AssertScopeValue(".campaign-create", "campaign.Name", Is.Null.Or.Empty);
			Navigation.GoToOutbound();
		}

		[When(@"I submit the form to create the campaign")]
		public void WhenISubmitTheFormToCreateTheCampaign()
		{
			ScenarioContext.Current.Pending();
		}

	
		[Then(@"I should see campaign details with")]
		public void ThenIShouldSeeCampaignDetailsWith(Table table)
		{
			var whatISee = table.CreateInstance<OutboundCampaignConfigurable>();		
			Browser.Interactions.AssertInputValue(".campaign-details #name", whatISee.Name);	
			Browser.Interactions.AssertFirstContains(".campaign-details .campaign-startdate", whatISee.StartDate.ToString("MMM d, yyyy"));
			Browser.Interactions.AssertFirstContains(".campaign-details .campaign-enddate", whatISee.EndDate.ToString("MMM d, yyyy"));
		}

		[When(@"I submit new working period with start time '(.*)' and end time '(.*)'")]
		public void WhenISubmitNewWorkingPeriodWithStartTimeAndEndTime(string startTime, string endTime)
		{
			Browser.Interactions.FillWith("input[name='NewTimeRangeStart']", startTime);
			Browser.Interactions.FillWith("input[name='NewTimeRangeEnd']", endTime);
			Browser.Interactions.Click(".new-working-period-submit");
		}

		[Then(@"I should see working period in the list with start time '(.*)' and end time '(.*)'")]
		public void ThenIShouldSeeWorkingPeriodInTheListWithStartTimeAndEndTime(string startTime, string endTime)
		{
			Browser.Interactions.AssertAnyContains(".campaign-working-hours li span", startTime);
			Browser.Interactions.AssertAnyContains(".campaign-working-hours li span", endTime);

		}

		[When(@"I change the campaign name to '(.*)'")]
		public void WhenIChangeTheCampaignNameTo(string campaignName)
		{
			Browser.Interactions.FillWith(".campaign-details #name", campaignName);
			Browser.Interactions.PressEnter(".campaign-details #name");
		}

		[When(@"I change the campaign start date to '(.*)' of the same month")]
		public void WhenIChangeTheCampaignStartDateToOfTheSameMonth(string day)
		{
			Browser.Interactions.ClickContaining(".campaign-startdate button", day);
		}

		[When(@"I delete '(.*)' from campaign list")]
		public void WhenIDeleteFromCampaignList(string campaignName)
		{
			Browser.Interactions.HoverOver(".campaign-list li", campaignName);
			Browser.Interactions.HoverOver(".campaign-list li .delete-campaign-toggle");
			Browser.Interactions.Click(".campaign-list .delete-campaign-toggle");
			Browser.Interactions.ClickContaining(".modal-box a", "AGREE");
		}




		
	

	}
}
