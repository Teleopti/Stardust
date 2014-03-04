using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PersonAccountStepDefinition
	{
		[Given(@"I have a personal account with")]
		public void GivenIHaveAPersonalAccount(Table table)
		{
			var personAbsenceAccountConfig = table.CreateInstance<PersonAbsenceAccountConfigurable>();
			DataMaker.Data().Apply(new PersonAbsenceAccountConfigurable(personAbsenceAccountConfig));
		}

		[Then(@"I should see the remaining days is '(.*)'")]
		[When(@"I see the remaining time is '(.*)'")]
		[Then(@"I should see the remaining time is '(.*)'")]
		public void ThenIShouldSeeTheRemainingTimeIs(string remainingTime)
		{
			Browser.Interactions.AssertExists(string.Format(".remainingTime :contains('{0}']", remainingTime));
		}

		[Then(@"I should see the used days is '(.*)'")]
		[When(@"I see the used time is '(.*)'")]
		[Then(@"I should see the used time is '(.*)'")]
		public void ThenIShouldSeeTheUsedTimeIs(string usedTime)
		{
			Browser.Interactions.AssertExists(string.Format(".remainingTime :contains('{0}']", usedTime));
		}
	}
}
