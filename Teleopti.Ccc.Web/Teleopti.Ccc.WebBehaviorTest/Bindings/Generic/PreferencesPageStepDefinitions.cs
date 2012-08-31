using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PreferencesPageStepDefinitions
	{
		[When(@"I select day '(.*)'")]
		public void WhenISelectDayDate(DateTime date)
		{
			Pages.Pages.PreferencePage.SelectCalendarCellByClass(date);
		}

		[When(@"I click the add extended preference button")]
		public void WhenIClickTheAddExtendedPreferenceButton()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceButton.EventualClick();
		}

		[When(@"I input extended preference fields with")]
		public void WhenIInputExtendedPreferenceFieldsWith(Table table)
		{
			var fields = table.CreateInstance<ExtendedPreferenceFields>();
			if (fields.Preference == null)
				ScenarioContext.Current.Pending();
			Pages.Pages.PreferencePage.ExtendedPreferenceSelectBox.Select(fields.Preference);
		}

		private class ExtendedPreferenceFields
		{
			public string Preference { get; set; }
		}

		[When(@"I click the apply extended preferences button")]
		public void WhenIClickTheApplyButton()
		{
			Pages.Pages.PreferencePage.ExtendedPreferenceApplyButton.EventualClick();
		}

		[When(@"I click the extended preference indication on '(.*)'")]
		public void WhenIClickTheExtendedPreferenceIndicationOn(DateTime date)
		{
			var indication = Pages.Pages.PreferencePage.ExtendedPreferenceIndicationForDate(date);
			indication.EventualClick();
		}

		[Then(@"I should see that I have an extended preference on '(.*)'")]
		[Then(@"I should see an extended preference indication on '(.*)'")]
		public void ThenIShouldSeeThatIHaveAnExtendedPreferenceOn(DateTime date)
		{
			var indication = Pages.Pages.PreferencePage.ExtendedPreferenceIndicationForDate(date);
			EventualAssert.That(() => indication.Exists, Is.True);
			EventualAssert.That(() => indication.DisplayVisible(), Is.True);
		}

		[Then(@"I should not see an extended preference indication on '(.*)'")]
		public void ThenIShouldNotSeeAnExtendedPreferenceIndicationOn(DateTime date)
		{
			var indication = Pages.Pages.PreferencePage.ExtendedPreferenceIndicationForDate(date);
			EventualAssert.That(() => indication.DisplayVisible(), Is.False);
		}

		[Then(@"I should see the extended preference on '(.*)'")]
		public void ThenIShouldSeeTheExtendedPreferenceOn(DateTime date)
		{
			var extendedPreference = Pages.Pages.PreferencePage.ExtendedPreferenceForDate(date);
			EventualAssert.That(() => extendedPreference.Exists, Is.True);
			EventualAssert.That(() => extendedPreference.JQueryVisible(), Is.True);
			EventualAssert.That(() => extendedPreference.DisplayVisible(), Is.True);
		}

		[Then(@"I should see the preference '(.*)' on '(.*)'")]
		public void ThenIShouldSeeThePreferenceLateOn(string preference, DateTime date)
		{
			var cell = Pages.Pages.PreferencePage.CalendarCellForDate(date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(preference));
		}

	}

}