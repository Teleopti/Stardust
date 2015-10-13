using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Outbound
{
	[Binding]
	public class OutboundCampaignStepDefinitions
	{
		[Then(@"I should see '(.*)' in campaign list")]
		public void ThenIShouldSeeInCampaignList(string campaignName)
		{
			Browser.Interactions.AssertAnyContains(".campaign-list", campaignName);
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

		[Then(@"I should not see '(.*)' in campaign list")]
		public void ThenIShouldNotSeeInCampaignList(string campaignName)
		{
			Browser.Interactions.AssertNoContains(".campaign-list li", ".campaign-list li", campaignName);
		}


	}
}
