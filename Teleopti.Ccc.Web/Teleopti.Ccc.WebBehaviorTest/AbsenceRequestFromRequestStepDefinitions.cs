using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AbsenceRequestFromRequestStepDefinitions 
	{
		[Then(@"I should see the absence request in the list")]
		public void ThenIShouldSeeTheAbsenceRequestInTheList()
		{
			ScenarioContext.Current.Pending();
		}

	}
}
