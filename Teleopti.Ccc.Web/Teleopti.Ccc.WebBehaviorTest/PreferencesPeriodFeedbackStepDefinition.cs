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

		[Given(@"I have a day off scheduled on weekday (\d)")]
		public void GivenIHaveADayOffScheduledOnWeekday3(int thOfDay)
		{
			ScenarioContext.Current.Pending();
		}

		[Given(@"I have a day off preference on weekday (\d)")]
		public void GivenIHaveADayOffPreferenceOnWeekday3(int weekday)
		{
			UserFactory.User().Setup(new DayOffPreference(weekday));
		}

		[Given(@"I have a contract with:")]
		public void GivenIHaveAContractWith(Table table)
		{
			var contract = table.CreateInstance<ContractFromTable>();
			UserFactory.User().Setup(contract);
			UserFactory.User().UserData<PersonPeriod>().Contract = contract;
		}

		[Given(@"I have a contract schedule with weekday (\d) day off")]
		public void GivenIHaveAContractScheduleWithWeekday6DayOff(int thOfDay)
		{
			ScenarioContext.Current.Pending();
		}

		[Given(@"I have a absence preference on weekday (\d)")]
		public void GivenIHaveAAbsencePreferenceOnWeekday5(int thOfDay)
		{
			ScenarioContext.Current.Pending();
		}





		[Then(@"I should see a message that I should have (\d) days off")]
		public void ThenIShouldSeeAMessageThatIShouldHave2DaysOff(int numOfDaysoff)
		{
			ScenarioContext.Current.Pending();
			EventualAssert.That(() => _page.PreferencePeriodFeedbackTargetDaysOff.Text, Is.StringContaining(string.Format(UserTexts.Resources.YouShouldHaveXDaysOff, numOfDaysoff)));
		}

		[Then(@"I should see a message that I should have between (\d) and (\d) days off")]
		public void ThenIShouldSeeAMessageThatIShouldHaveBetweenXAndYDaysOff(int lower, int upper)
		{
			ScenarioContext.Current.Pending();
			EventualAssert.That(() => _page.PreferencePeriodFeedbackTargetDaysOff.Text, Is.StringContaining(string.Format(UserTexts.Resources.YouShouldHaveBetweenXAndYDaysOff, lower, upper)));
		}

		[Then(@"I should see a message that my preferences can result (\d) days off")]
		public void ThenIShouldSeeAMessageThatMyPreferencesCanResult2DaysOff(int daysoff)
		{
			ScenarioContext.Current.Pending();
			EventualAssert.That(() => _page.PreferencePeriodFeedbackPossibleResultDaysOff.Text, Is.StringContaining(string.Format(UserTexts.Resources.YourPreferencesCanResultXDaysOff, daysoff)));
		}

		[Then(@"I should see a message that I should work (\d+) hours")]
		public void ThenIShouldSeeAMessageThatIShouldWorkXHours(int hours)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackTargetHours.Text, Is.StringContaining(string.Format(UserTexts.Resources.YouShouldWorkXHours, hours)));
		}

	}

}