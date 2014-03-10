using System.Globalization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		[Given(@"The absence account is updated")]
		public void GivenTheAbsenceAccountIsUpdated()
		{
			var currentScenario = new ThisCurrentScenario(GlobalDataMaker.Data().Data<CommonScenario>().Scenario);

			var personAccountUpdateConfigurable = new PersonAccountUpdateConfigurable { CurrentScenario = currentScenario };
			DataMaker.Data().Apply(personAccountUpdateConfigurable);
		}

		[When(@"I see the remaining time is '(.*)'")]
		[Then(@"I should see the remaining days is '(.*)'")]
		[Then(@"I should see the remaining time is '(.*)'")]
		public void ThenIShouldSeeTheRemainingTimeIs(string remainingTime)
		{
			Browser.Interactions.AssertAnyContains(".remainingTime", remainingTime);
		}

		[When(@"I see the used time is '(.*)'")]
		[Then(@"I should see the used days is '(.*)'")]
		[Then(@"I should see the used time is '(.*)'")]
		public void ThenIShouldSeeTheUsedTimeIs(string usedTime)
		{
			Browser.Interactions.AssertAnyContains(".usedTime", usedTime);
		}
	}


}