using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class PreferencesPeriodFeedbackStepDefinition
	{
		private PreferencePage _page { get { return Pages.Pages.PreferencePage; } }

		[Given(@"I have a scheduling period of 1 week")]
		public void GivenIHaveASchedulingPeriodOf1Week()
		{
			UserFactory.User().ReplaceSetupByType<SchedulePeriod>(new SchedulePeriod(0, 1));
		}

		[Given(@"I have a contract schedule with 2 days off")]
		public void GivenIHaveAContractScheduleWith2DaysOff()
		{
			var contractSchedule = GlobalDataContext.Data().Data<CommonContractSchedule>();
			UserFactory.User().Setup(new UserContractSchedule(contractSchedule.ContractSchedule));
		}

		[Given(@"I have a scheduled day off on weekday (\d)")]
		[Given(@"I have a day off scheduled on weekday (\d)")]
		public void GivenIHaveADayOffScheduledOnWeekday3(int weekday)
		{
			UserFactory.User().Setup(new DayOffScheduled(weekday));
		}
		
		[Given(@"I have a day off preference on weekday (\d)")]
		public void GivenIHaveADayOffPreferenceOnWeekday3(int weekday)
		{
			UserFactory.User().Setup(new DayOffPreferenceOnWeekday(weekday));
		}

		[Given(@"I have a contract with:")]
		public void GivenIHaveAContractWith(Table table)
		{
			var contract = table.CreateInstance<ContractFromTable>();
			UserFactory.User().Setup(contract);
			UserFactory.User().UserData<PersonPeriod>().Contract = contract;
		}

		[Given(@"I have a contract schedule with:")]
		public void GivenIHaveAContractScheduleWith(Table table)
		{
			var contractSchedule = table.CreateInstance<ContractScheduleFromTable>();
			UserFactory.User().Setup(contractSchedule);
			UserFactory.User().UserData<PersonPeriod>().ContractSchedule = contractSchedule;
		}
		
		[Given(@"I have a absence preference on weekday (\d)")]
		[Given(@"I have a non-contract time absence preference on weekday (\d)")]
		public void GivenIHaveAAbsencePreferenceOnWeekdayX(int weekday)
		{
			UserFactory.User().Setup(new AbsencePreferenceOnWeekday(weekday));
		}

		[Given(@"I have a contract time absence preference on weekday (\d)")]
		public void GivenIHaveAContractTimeAbsencePreferenceOnWeekdayX(int weekday)
		{
			UserFactory.User().Setup(new AbsencePreferenceInContractTimeOnWeekday(weekday));
		}

		[Given(@"I have a shift category preference on weekday (\d)")]
		public void GivenIHaveAShiftCategoryPreferenceOnWeekday1(int weekday)
		{
			UserFactory.User().Setup(new ShiftCategoryPreferenceOnWeekday(weekday));
		}

		[Given(@"I have a scheduled shift of (\d) hours on weekday (\d)")]
		public void GivenIHaveAScheduledShiftOf8HoursOnWeekday1(int hours, int weekday)
		{
			UserFactory.User().Setup(new ShiftOnWeekday(TimeSpan.FromHours(8), TimeSpan.FromHours(8 + hours), weekday));
		}





		[Then(@"I should see a message that I should have (\d) days off")]
		public void ThenIShouldSeeAMessageThatIShouldHave2DaysOff(int numOfDaysoff)
		{
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", string.Format(UserTexts.Resources.YouShouldHaveXDaysOff, numOfDaysoff));
		}

		[Then(@"I should see a message that I should have between (\d) and (\d) days off")]
		public void ThenIShouldSeeAMessageThatIShouldHaveBetweenXAndYDaysOff(int lower, int upper)
		{
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", string.Format(UserTexts.Resources.YouShouldHaveBetweenXAndYDaysOff, lower, upper));
		}

		[Then(@"I should see a message that my preferences can result (\d) days off")]
		public void ThenIShouldSeeAMessageThatMyPreferencesCanResult2DaysOff(int daysoff)
		{
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", string.Format(UserTexts.Resources.YourPreferencesCanResultXDaysOff, daysoff));
		}

		[Then(@"I should see a message that I should work (\d+) hours")]
		public void ThenIShouldSeeAMessageThatIShouldWorkXHours(int hours)
		{
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", string.Format(UserTexts.Resources.YouShouldWorkXHours, FormatHours(hours)));
		}

		[Then(@"I should see a message that I should work (\d+) to (\d+) hours")]
		public void ThenIShouldSeeAMessageThatIShouldWorkXToYHours(int lower, int upper)
		{
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", string.Format(UserTexts.Resources.YouShouldWorkBetweenXAndYHours, FormatHours(lower), FormatHours(upper)));
		}

		[Then(@"I should see a message that my preferences can result in (\d+) to (\d+) hours")]
		public void ThenIShouldSeeAMessageThatMyPreferencesCanResultInXToYHours(int lower, int upper)
		{
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", string.Format(UserTexts.Resources.YourPreferencesCanResultXToYHours, FormatHours(lower), FormatHours(upper)));
		}

		[Then(@"I should see a message that my preferences can result in (\d+) hours")]
		public void ThenIShouldSeeAMessageThatMyPreferencesCanResultInXHours(int hours)
		{
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", string.Format(UserTexts.Resources.YourPreferencesCanResultXHours, FormatHours(hours)));
		}

		[Then(@"I should see a warning for my dayoff preferences outside the target")]
		public void ThenIShouldSeeAWarningForMyDayoffPreferencesOutsideTheTarget()
		{
			Browser.Interactions.AssertExists("#Preference-period-feedback-view div:nth-of-type(2) span.icon-exclamation-sign");
		}

		[Then(@"I should not see a warning for my dayoff preferences outside the target")]
		public void ThenIShouldNotSeeAWarningForMyDayoffPreferencesOutsideTheTarget()
		{
			Browser.Interactions.AssertNotExists("#Preference-period-feedback-view div:nth-of-type(2)", "#Preference-period-feedback-view div:nth-of-type(2) span.icon-exclamation-sign");
		}

		[Then(@"I should see a warning for my time preferences outside the target")]
		public void ThenIShouldSeeAWarningForMyTimePreferencesOutsideTheTarget()
		{
			Browser.Interactions.AssertExists("#Preference-period-feedback-view div:nth-of-type(1) span.icon-exclamation-sign");
		}

		[Then(@"I should not see a warning for my time preferences outside the target")]
		public void ThenIShouldNotSeeAWarningForMyTimePreferencesOutsideTheTarget()
		{
			Browser.Interactions.AssertNotExists("#Preference-period-feedback-view div:nth-of-type(1)", "#Preference-period-feedback-view div:nth-of-type(1) span.icon-exclamation-sign");
		}


		private static string FormatHours(int hours)
		{
			return string.Format("{0}:00", hours);
		}
	}
}