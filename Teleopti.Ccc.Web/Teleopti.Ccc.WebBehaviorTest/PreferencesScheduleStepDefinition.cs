using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;

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

		[Given(@"I have a contract schedule with (\d) days off")]
		public void GivenIHaveAContractScheduleWith2DaysOff(int daysoff)
		{
			//UserFactory.User().Setup(new DayOffPreference());
			//UserFactory.User().Setup(new DayOffPreference());
		}

		[Given(@"I have a day off scheduled on weekday (\d)")]
		public void GivenIHaveADayOffScheduledOnWeekday3(int thOfDay)
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see a message that I should have (\d) days off")]
		public void ThenIShouldSeeAMessageThatIShouldHave2DaysOff(int numOfDaysoff)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackShouldHave.InnerHtml, Is.Not.StringContaining(numOfDaysoff + " days off"));
		}

		[Given(@"I have a day off preference on weekday (\d)")]
		public void GivenIHaveADayOffPreferenceOnWeekday3(int thOfDay)
		{
			UserFactory.User().Setup(new DayOffPreference(thOfDay));
		}

		[Then(@"I should see a message that my preferences can result (\d) days off")]
		public void ThenIShouldSeeAMessageThatMyPreferencesCanResult2DaysOff(int numOfDaysoff)
		{
			EventualAssert.That(() => _page.PreferencePeriodFeedbackCanResult.InnerHtml, Is.Not.StringContaining(numOfDaysoff + " days off"));
		}

		[Given(@"I have a contract with a day off tolerance of negative 1 days")]
		public void GivenIHaveAContractWithADayOffToleranceOfNegative1Days()
		{
			ScenarioContext.Current.Pending();
		}

		[Given(@"I have a contract with a day off tolerance of positive 1 days")]
		public void GivenIHaveAContractWithADayOffToleranceOfPositive1Days()
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see a message that I should have between 1 and 3 days off")]
		public void ThenIShouldSeeAMessageThatIShouldHaveBetween1And3DaysOff()
		{
			ScenarioContext.Current.Pending();
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




	}
}