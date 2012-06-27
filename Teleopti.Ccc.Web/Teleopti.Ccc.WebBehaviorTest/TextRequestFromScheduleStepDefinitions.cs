using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class TextRequestFromScheduleStepDefinitions
	{
		[When(@"I click on today's summary")]
		public void WhenIClickOnTodaySSummary()
		{
			Pages.Pages.WeekSchedulePage.DayElementForDate(DateTime.Today).ListItems.First(Find.ById("day-summary")).EventualClick();
		}

		[When(@"I click on tomorrows summary")]
		public void WhenIClickOnTomorrowsSummary()
		{
			Pages.Pages.WeekSchedulePage.DayElementForDate(DateTime.Today.AddDays(1)).ListItems.First(Find.ByClass("add-text-request")).EventualClick();
		}

		[Then(@"I should see the text request form")]
		public void ThenIShouldSeeTheTextRequestForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.True);
		}

		[Then(@"I should see the text request form with tomorrow as default date")]
		public void ThenIShouldSeeTheTextRequestFormWithTomorrowAsDefaultDate()
		{
			var tomorrow = DateTime.Today.AddDays(1);
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.TextRequestDetailFromDateTextField.Value), Is.EqualTo(tomorrow));
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.TextRequestDetailToDateTextField.Value), Is.EqualTo(tomorrow));
		}

		[Then(@"I should not see the text request form")]
		public void ThenIShouldNotSeeTheTextRequestForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.False);
		}


	}
}
