using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class EditRequestPageStepDefinitions
	{
		[When(@"I click add request button in the toolbar")]
		public void WhenIClickAddRequestButtonInTheToolbar()
		{
			Pages.Pages.CurrentEditRequestPage.AddRequestButton.EventualClick();
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
		}

		[When(@"I click absence request tab")]
		public void WhenIClickAbsenceRequestTab()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.EventualClick();
		}

		[Then(@"I should not see the absence request tab")]
		public void ThenIShouldNotSeeTheAbsenceRequestTab()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.Exists, Is.False);
		}

		[Then(@"I should not see the absence request tab \(invisible\)")]
		public void ThenIShouldNotSeeTheAbsenceRequestTabInvisible()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.DisplayHidden(), Is.True);
		}

		[Then(@"I should see the absence request tab")]
		public void ThenIShouldSeeTheAbsenceRequestTab()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.Exists, Is.True);
		}

		[Then(@"I should see the text request form")]
		public void ThenIShouldSeeTheTextRequestForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.True);
		}

        [Then(@"I should see the text request form with '(.*)' as default date")]
        public void ThenIShouldSeeTheTextRequestFormWithAsDefaultDate(DateTime date)
        {
            EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value), Is.EqualTo(date));
            EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value), Is.EqualTo(date));
        }

		[Then(@"I should not see the text request form")]
		public void ThenIShouldNotSeeTheTextRequestForm()
		{
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.False);
		}
	}
}
