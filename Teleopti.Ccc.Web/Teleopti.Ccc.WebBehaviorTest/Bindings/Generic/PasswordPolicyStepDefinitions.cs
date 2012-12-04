using System.IO;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PasswordPolicyStepDefinitions
	{
		[Given(@"There is a password policy with")]
		public void GivenThereIsAPasswordPolicyWith(Table table)
		{
			var targetTestPasswordPolicyFile = Path.Combine(Path.Combine(IniFileInfo.SitePath, "bin"), "PasswordPolicy.xml");
			var contents = File.ReadAllText("Data\\PasswordPolicy.xml");
			var passwordPolicy = table.CreateInstance<PasswordPolicyConfigurable>();

			contents = contents.Replace("_MaxNumberOfAttempts_", passwordPolicy.MaxNumberOfAttempts.ToString());
			contents = contents.Replace("_InvalidAttemptWindow_", passwordPolicy.InvalidAttemptWindow.ToString());
			contents = contents.Replace("_PasswordValidForDayCount_", passwordPolicy.PasswordValidForDayCount.ToString());
			contents = contents.Replace("_PasswordExpireWarningDayCount_", passwordPolicy.PasswordExpireWarningDayCount.ToString());

			if (passwordPolicy.Rule1.Equals("PasswordLengthMin8"))
			{
			}

			File.WriteAllText(targetTestPasswordPolicyFile, contents);
		}

		[Given(@"I have user logon details with")]
		public void GivenIHaveUserLogonDetailsWith(Table table)
		{
			var userLogonDetai = table.CreateInstance<UserLogonDetailConfigurable>();
			UserFactory.User().Setup(userLogonDetai);
		}
	}
}