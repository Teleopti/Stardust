﻿using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using PersonAbsenceConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonAbsenceConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{

	[Scope(Feature = "Absence request from requests")]
	[Scope(Feature = "Absence request from schedule")]
	[Binding]
	public class PersonAccountStepDefinition
	{
		[Given(@"I have a personal account with")]
		public void GivenIHaveAPersonalAccount(Table table)
		{
			var personAbsenceAccountConfig = table.CreateInstance<PersonAbsenceAccountConfigurable>();
			DataMaker.Data().Apply(new PersonAbsenceAccountConfigurable(personAbsenceAccountConfig));
		}

		[Given(@"'?(I)'? have an absence with")]
		[Given(@"'?(.*)'? has an absence with")]
		public void GivenHaveAAbsenceWith(string userName, Table table)
		{
			DataMaker.ApplyFromTable<PersonAbsenceConfigurable>(userName, table);
			var currentScenario = new ThisCurrentScenario(DefaultScenario.Scenario);

			var personAccountUpdateConfigurable = new PersonAccountUpdateConfigurable { CurrentScenario = currentScenario };
			DataMaker.Data().Apply(personAccountUpdateConfigurable);
		}

		[Given(@"I see the remaining time is '(.*)'")]
		[When(@"I see the remaining time is '(.*)'")]
		[Then(@"I should see the remaining days is '(.*)'")]
		[Then(@"I should see the remaining time is '(.*)'")]
		public void ThenIShouldSeeTheRemainingTimeIs(string remainingTime)
		{
			Browser.Interactions.AssertExists("#absence-personal-account");
			Browser.Interactions.AssertAnyContains(".remainingTime", remainingTime);
		}

		[Given(@"I see the used time is '(.*)'")]
		[When(@"I see the used time is '(.*)'")]
		[Then(@"I should see the used days is '(.*)'")]
		[Then(@"I should see the used time is '(.*)'")]
		public void ThenIShouldSeeTheUsedTimeIs(string usedTime)
		{
			Browser.Interactions.AssertExists("#absence-personal-account");
			Browser.Interactions.AssertAnyContains(".usedTime", usedTime);
		}

		[Then(@"I should not see the remaining and used time")]
		public void ThenIShouldNotSeeTheRemainingAndUsedTime()
		{
			Browser.Interactions.AssertNotExists(".form-control", "#absence-personal-account");
		}
	}
}