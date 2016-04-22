using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.TeamSchedule
{
	[Binding]
	public sealed class TeamScheduleStepDefinitions
	{
		[When(@"I searched schedule with keyword '(.*)' and schedule date '(.*)'")]
		public void WhenISearchedScheduleWithKeywordAndScheduleDate(string searchkeyword, DateTime scheduleDate)
		{
			Browser.Interactions.FillWith("#simple-people-search", string.Format("\"{0}\"", searchkeyword));

			// xinfli: Not sure why set date directly not trigged load schedule,
			// so I set it to previous day and switched to next day by click button.
			var propertyValues = new Dictionary<string, string>
			{
				{"vm.scheduleDate", string.Format("new Date('{0}')", scheduleDate.AddDays(-1).ToShortDateString())}
			};
			Browser.Interactions.SetScopeValues(".datepicker-container", propertyValues);
			Browser.Interactions.ClickUsingJQuery(".datepicker-container button:has('i.mdi-chevron-double-right')");
		}

		[Then(@"I should see schedule with absence for '(.*)' displayed")]
		public void ThenIShouldSeeScheduleForDisplayed(string agentName)
		{
			Browser.Interactions.AssertAnyContains(".person-name", agentName);
			Browser.Interactions.AssertExists(".schedule div.personAbsence");
		}

		[Then(@"I should see schedule with no absence for '(.*)' displayed")]
		public void ThenIShouldSeeScheduleWithNoAbsenceForDisplayed(string agentName)
		{
			Browser.Interactions.AssertAnyContains(".person-name", agentName);
			Browser.Interactions.AssertNotExists(".person-name", ".schedule div.personAbsence");
		}

		[When(@"I selected the person absence for '(.*)'")]
		public void WhenISelectedThePersonAbsenceFor(string agentName)
		{
			Browser.Interactions.Click(".schedule div.personAbsence");
		}

		[When(@"I selected agent '(.*)'")]
		public void WhenISelectedAgent(string agentName)
		{
			Browser.Interactions.ClickContaining(".person-name", agentName);
		}
		
		[When(@"I open '(.*)' panel")]
		public void WhenIOpenPanel(string panelName)
		{
			Browser.Interactions.Click("#scheduleContextMenuButton");
			Browser.Interactions.Click("#menuItem" + panelName);
		}

		[When(@"I set new activity as")]
		public void WhenISetNewActivityAs(Table table)
		{
			var values = table.CreateInstance<AddActivityFormInfo>();

			Browser.Interactions.ClickContaining(".activity-selector option", values.Activity);
			var startTime = string.Format("new Date((new Date()).toDateString()+ ' {0}')",
				values.StartTime.ToShortTimeString(DataMaker.Me().Culture));
			var endTime = string.Format("new Date((new Date()).toDateString()+ ' {0}')",
				values.EndTime.ToShortTimeString(DataMaker.Me().Culture));
			var timeRangeStr = string.Format("{{startTime:{0}, endTime:{1}}}", startTime, endTime);
			var timeRange = new Dictionary<string, string>
			{
				{"vm.timeRange", timeRangeStr}
			};
			Browser.Interactions.SetScopeValues(".activity-time-range", timeRange);
		}

		[Then(@"I should be able to apply my new activity")]
		public void ThenIShouldBeAbleToApplyMyNewActivity()
		{
			Browser.Interactions.AssertScopeValue("#applyActivity", "newActivityForm.$valid", true);
			Browser.Interactions.AssertExists("#applyActivity.wfm-btn-primary");
		}

		[When(@"I apply my new activity")]
		public void WhenIApplyMyNewActivity()
		{
			Browser.Interactions.Click("#applyActivity");
		}

		[Then(@"I should see a successful notice")]
		public void ThenIShouldSeeASuccessfulNotice()
		{
			Browser.Interactions.AssertExists(".mdi-thumb-up");
		}
		
		[When(@"I try to delete selected absence")]
		public void WhenITryToDeleteSelectedAbsence()
		{
			var propertyValues = new Dictionary<string, string>
			{
				{"vm.isScenarioTest", "true"}
			};
			Browser.Interactions.SetScopeValues(".scenario-test-trick", propertyValues);
			Browser.Interactions.ClickUsingJQuery("#scheduleContextMenuButton");
			Browser.Interactions.ClickUsingJQuery("#menuItemRemoveAbsence");
		}

		[Then(@"I should see a confirm message that will remove (\d*) absences from (\d*) person"), SetCulture("en-US")]
		public void ThenIShouldSeeDialogToConfirmAbsenceDeletion(int personCount, int personAbsenceCount)
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
			Browser.Interactions.AssertAnyContains(".team-schedule-command-confirm-dialog",
				string.Format(Resources.AreYouSureToRemoveSelectedAbsence, personAbsenceCount, personCount));
		}

		[When(@"I click apply button")]
		public void WhenIClickApplyButton()
		{
			Browser.Interactions.Click(".team-schedule-command-confirm-dialog .wfm-btn-primary:not([disabled])");
		}

	}

	public class AddActivityFormInfo
	{
		public string Activity { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public Boolean IsNextDay { get; set; }
	}
}
