using System;
using System.IO;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PasswordPolicyStepDefinitions
	{
		private IDisposable _timeoutScope;

		[BeforeScenario("PasswordPolicy")]
		public void BeforePasswordPolicyScenario()
		{
			_timeoutScope = Browser.TimeoutScope(TimeSpan.FromSeconds(20));
		}

		[AfterScenario("PasswordPolicy")]
		public void AfterPasswordPolicyScenario()
		{
			var targetTestPasswordPolicyFile = Path.Combine(IniFileInfo.SitePath, "PasswordPolicy.xml");
			if (File.Exists(targetTestPasswordPolicyFile))
				File.Delete(targetTestPasswordPolicyFile);
			_timeoutScope.Dispose();
			_timeoutScope = null;
		}

		[Given(@"There is a password policy with")]
		public void GivenThereIsAPasswordPolicyWith(Table table)
		{
			var targetTestPasswordPolicyFile = Path.Combine(IniFileInfo.SitePath, "PasswordPolicy.xml");
			if (File.Exists(targetTestPasswordPolicyFile))
				return;
			var contents = File.ReadAllText("Data\\PasswordPolicy.xml");
			var passwordPolicy = table.CreateInstance<PasswordPolicyInfo>();

			contents = contents.Replace("_MaxNumberOfAttempts_", passwordPolicy.MaxNumberOfAttempts.ToString());
			contents = contents.Replace("_InvalidAttemptWindow_", passwordPolicy.InvalidAttemptWindow.ToString());
			contents = contents.Replace("_PasswordValidForDayCount_", passwordPolicy.PasswordValidForDayCount.ToString());
			contents = contents.Replace("_PasswordExpireWarningDayCount_", passwordPolicy.PasswordExpireWarningDayCount.ToString());

			if (passwordPolicy.Rule1.Equals("PasswordLengthMin8"))
			{
			}

			File.WriteAllText(targetTestPasswordPolicyFile, contents);
		}

		public class PasswordPolicyInfo
		{
			public int MaxNumberOfAttempts { get; set; }
			public int InvalidAttemptWindow { get; set; }
			public int PasswordValidForDayCount { get; set; }
			public int PasswordExpireWarningDayCount { get; set; }
			public string Rule1 { get; set; }
		}


	}
}