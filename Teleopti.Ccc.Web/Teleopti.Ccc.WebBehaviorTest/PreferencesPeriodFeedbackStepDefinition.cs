using System;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			var contractSchedule = DataContext.Data().Data<ContractScheduleWith2DaysOff>();
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
			EventualAssert.That(() => _page.PreferencePeriodFeedbackView.Text, Is.StringContaining(string.Format(UserTexts.Resources.YouShouldHaveXDaysOff, numOfDaysoff)));
		}

		[Then(@"I should see a message that I should have between (\d) and (\d) days off")]
		public void ThenIShouldSeeAMessageThatIShouldHaveBetweenXAndYDaysOff(int lower, int upper)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackView.Text, Is.StringContaining(string.Format(UserTexts.Resources.YouShouldHaveBetweenXAndYDaysOff, lower, upper)));
		}

		[Then(@"I should see a message that my preferences can result (\d) days off")]
		public void ThenIShouldSeeAMessageThatMyPreferencesCanResult2DaysOff(int daysoff)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackView.Text, Is.StringContaining(string.Format(UserTexts.Resources.YourPreferencesCanResultXDaysOff, daysoff)));
		}

		[Then(@"I should see a message that I should work (\d+) hours")]
		public void ThenIShouldSeeAMessageThatIShouldWorkXHours(int hours)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackView.Text, Is.StringContaining(string.Format(UserTexts.Resources.YouShouldWorkXHours, FormatHours(hours))));
		}

		[Then(@"I should see a message that I should work (\d+) to (\d+) hours")]
		public void ThenIShouldSeeAMessageThatIShouldWorkXToYHours(int lower, int upper)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackView.Text, Is.StringContaining(string.Format(UserTexts.Resources.YouShouldWorkBetweenXAndYHours, FormatHours(lower), FormatHours(upper))));
		}

		[Then(@"I should see a message that my preferences can result in (\d+) to (\d+) hours")]
		public void ThenIShouldSeeAMessageThatMyPreferencesCanResultInXToYHours(int lower, int upper)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackView.Text, Is.StringContaining(string.Format(UserTexts.Resources.YourPreferencesCanResultXToYHours, FormatHours(lower), FormatHours(upper))));
		}

		[Then(@"I should see a message that my preferences can result in (\d+) hours")]
		public void ThenIShouldSeeAMessageThatMyPreferencesCanResultInXHours(int hours)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackView.Text, Is.StringContaining(string.Format(UserTexts.Resources.YourPreferencesCanResultXHours, FormatHours(hours))));
		}

		private static string FormatHours(int hours)
		{
			return string.Format("{0}:00", hours);
		}
	}
}