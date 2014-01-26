using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferencesPeriodFeedbackStepDefinition
	{
		[Given(@"I have a scheduling period of 1 week")]
		public void GivenIHaveASchedulingPeriodOf1Week()
		{
			DataMaker.Data().Apply(new SchedulePeriod(0, 1));
		}

		[Given(@"I have a contract schedule with 2 days off")]
		public void GivenIHaveAContractScheduleWith2DaysOff()
		{
			var contractSchedule = GlobalDataMaker.Data().Data<CommonContractSchedule>();
			DataMaker.Data().Apply(new UserContractSchedule(contractSchedule.ContractSchedule));
		}

		[Given(@"I have a scheduled day off on weekday (\d)")]
		[Given(@"I have a day off scheduled on weekday (\d)")]
		public void GivenIHaveADayOffScheduledOnWeekday3(int weekday)
		{
			DataMaker.Data().Apply(new DayOffScheduled(weekday));
		}
		
		[Given(@"I have a day off preference on weekday (\d)")]
		public void GivenIHaveADayOffPreferenceOnWeekday3(int weekday)
		{
			DataMaker.Data().Apply(new DayOffPreferenceOnWeekday(weekday));
		}

		[Given(@"I have a contract with:")]
		public void GivenIHaveAContractWith(Table table)
		{
			var contract = table.CreateInstance<ContractFromTable>();
			DataMaker.Data().Apply(contract);
			DataMaker.Data().Apply(new UserContract(contract.Contract));
		}

		[Given(@"I have a contract schedule with:")]
		public void GivenIHaveAContractScheduleWith(Table table)
		{
			var contractSchedule = table.CreateInstance<ContractScheduleFromTable>();
			DataMaker.Data().Apply(contractSchedule);
			DataMaker.Data().Apply(new UserContractSchedule(contractSchedule.ContractSchedule));
		}
		
		[Given(@"I have a absence preference on weekday (\d)")]
		[Given(@"I have a non-contract time absence preference on weekday (\d)")]
		public void GivenIHaveAAbsencePreferenceOnWeekdayX(int weekday)
		{
			DataMaker.Data().Apply(new AbsencePreferenceOnWeekday(weekday));
		}

		[Given(@"I have a contract time absence preference on weekday (\d)")]
		public void GivenIHaveAContractTimeAbsencePreferenceOnWeekdayX(int weekday)
		{
			DataMaker.Data().Apply(new AbsencePreferenceInContractTimeOnWeekday(weekday));
		}

		[Given(@"I have a shift category preference on weekday (\d)")]
		public void GivenIHaveAShiftCategoryPreferenceOnWeekday1(int weekday)
		{
			DataMaker.Data().Apply(new ShiftCategoryPreferenceOnWeekday(weekday));
		}

		[Given(@"I have a scheduled shift of (\d) hours on weekday (\d)")]
		public void GivenIHaveAScheduledShiftOf8HoursOnWeekday1(int hours, int weekday)
		{
			DataMaker.Data().Apply(new ShiftOnWeekday(TimeSpan.FromHours(8), TimeSpan.FromHours(8 + hours), weekday));
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
			Browser.Interactions.AssertExists("#Preference-period-feedback-view div:nth-of-type(2) span.glyphicon-exclamation-sign");
		}

		[Then(@"I should not see a warning for my dayoff preferences outside the target")]
		public void ThenIShouldNotSeeAWarningForMyDayoffPreferencesOutsideTheTarget()
		{
			Browser.Interactions.AssertNotExists("#Preference-period-feedback-view div:nth-of-type(2)", "#Preference-period-feedback-view div:nth-of-type(2) span.glyphicon-exclamation-sign");
		}

		[Then(@"I should see a warning for my time preferences outside the target")]
		public void ThenIShouldSeeAWarningForMyTimePreferencesOutsideTheTarget()
		{
			Browser.Interactions.AssertExists("#Preference-period-feedback-view div:nth-of-type(1) span.glyphicon-exclamation-sign");
		}

		[Then(@"I should not see a warning for my time preferences outside the target")]
		public void ThenIShouldNotSeeAWarningForMyTimePreferencesOutsideTheTarget()
		{
			Browser.Interactions.AssertNotExists("#Preference-period-feedback-view div:nth-of-type(1)", "#Preference-period-feedback-view div:nth-of-type(1) span.glyphicon-exclamation-sign");
		}


		private static string FormatHours(int hours)
		{
			return string.Format("{0}:00", hours);
		}
	}
}