﻿using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AbsenceRequestFromScheduleStepDefinitions 
	{

		[Then(@"I should see an absence type called (.*) in droplist")]
		public void ThenIShouldSeeAAbsenceTypeCalledVacationInDroplist(string name)
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceTypesSelectList.InnerHtml, Is.StringContaining(name));
		}

		[When(@"I unchecked the full day checkbox")]
		public void WhenIUncheckedTheFullDayCheckbox()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.FulldayCheck.Checked = false;
		}

		[When(@"I checked the full day checkbox")]
		public void WhenIClickFullDayCheckbox()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.FulldayCheck.Checked = true;
		}

		[Then(@"I should see my existing inputs")]
		public void ThenIShouldSeeMyExistingInputs()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.TextRequestDetailSubjectInput.Value, Is.StringContaining("The cake is a.. Cake!"));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.TextRequestDetailMessageTextField.Value, Is.StringContaining("A message. A very very very short message. Or maybe not."));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.TextRequestDetailFromDateTextField.Value, Is.StringContaining(DateTime.Today.ToShortDateString(UserFactory.User().Culture)));
		}


		[Then(@"I should not see the absence request tab")]
		public void ThenIShouldNotSeeTheAbsenceRequestTab()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.Exists, Is.False);
		}

		[Given(@"I am an agent without access to absence requests")]
		public void GivenIAmAnAgentWithoutAccessToAbsenceRequests()
		{
			UserFactory.User().Setup(new AgentWithoutAbsenceRequestsAccess());
		}
	}
}
