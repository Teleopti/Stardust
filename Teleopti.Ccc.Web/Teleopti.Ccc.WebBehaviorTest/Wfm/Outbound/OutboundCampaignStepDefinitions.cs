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
			Browser.Interactions.WaitScopeCondition(".outbound-summary", "isRefreshingGantt", Is.EqualTo("false"),
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

			Browser.Interactions.WaitScopeCondition(".campaign-create", "isInputValid()", Is.EqualTo("true"), () => 
					Browser.Interactions.Click(".form-submit"));							
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
				Browser.Interactions.Click(".trigger-campaign-delete");
				Browser.Interactions.Click(".modal-box .confirm-delete");
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

			Browser.Interactions.WaitScopeCondition(".campaign-edit", "isInputValid()", Is.EqualTo("true"), () =>
					Browser.Interactions.Click(".form-submit"));	
		}

		[When(@"after the update is done I goto the campaign list page")]
		public void WhenAfterTheUpdateIsDoneIGotoTheCampaignListPage()
		{
			Browser.Interactions.AssertScopeValue(".campaign-edit", "campaignSpanningPeriodForm.$pristine", Is.EqualTo("true"));
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
			Browser.Interactions.Click(".btn-goto-edit-campaign");
		}
	
	}
}
