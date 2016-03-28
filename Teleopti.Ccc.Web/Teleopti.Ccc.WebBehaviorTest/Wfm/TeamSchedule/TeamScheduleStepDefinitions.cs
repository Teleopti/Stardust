﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.UserTexts;

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
			Browser.Interactions.AssertAnyContains(".modal-dialog",
				string.Format(Resources.AreYouSureToRemoveSelectedAbsence, personAbsenceCount, personCount));
		}

		[When(@"I answered '(.*)' to confirm message")]
		public void WhenIAnsweredToConfirmDialog(string buttonText)
		{
			Browser.Interactions.ClickContaining(".modal-dialog button", buttonText);
		}
	}
}
