using System;
using log4net;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PasswordPolicyStepDefinitions
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(PasswordPolicyStepDefinitions));
		
		[Given(@"There is a password policy with")]
		public void GivenThereIsAPasswordPolicyWith(Table table)
		{
			// Handled within the test controller when a policy is enabled through UsePasswordPolicy argument.
		}
	}
}