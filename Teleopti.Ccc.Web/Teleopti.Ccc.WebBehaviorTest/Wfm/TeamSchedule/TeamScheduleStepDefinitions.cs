using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

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
			Browser.Interactions.Click(".datepicker-container button>i.mdi-chevron-double-right");
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
		
		[When(@"I try to delete selected absence")]
		public void WhenITryToDeleteSelectedAbsence()
		{
			Browser.Interactions.Click("#scheduleContextMenuButton");
			Browser.Interactions.Click("#menuItemRemoveAbsence");
		}

		[Then(@"I should see a confirm message for absence deletion")]
		public void ThenIShouldSeeDialogToConfirmAbsenceDeletion()
		{
			Browser.Interactions.AssertAnyContains(".modal-dialog", "Are you sure to delete all selected absences?");
		}

		[When(@"I answered '(.*)' to confirm message")]
		public void WhenIAnsweredToConfirmDialog(string buttonText)
		{
			Browser.Interactions.ClickContaining(".modal-dialog button", buttonText);
		}
	}
}
