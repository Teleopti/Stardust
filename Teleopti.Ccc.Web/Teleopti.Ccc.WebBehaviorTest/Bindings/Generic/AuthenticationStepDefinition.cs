﻿using System.IO;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AuthenticationStepDefinition
	{
		private static readonly string TargetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData2.nhib.xml");

		[Given(@"I have access to two data sources")]
		public void GivenIHaveAccessToTwoDataSources()
		{
		}

		[When(@"I select the first data source")]
		public void WhenISelectTheFirstDataSource()
		{
			Browser.Interactions.ClickUsingJQuery("#DataSources a:first");
		}

		[BeforeScenario("ExtraDataSource")]
		public void BeforePasswordPolicyScenario()
		{
			FileConfigurator.ConfigureByTags(
				"Data\\TestData2.nhib.xml",
				TargetTestDataNHibFile,
				new AllTags()
				);
		}

		[AfterScenario("ExtraDataSource")]
		public void AfterPasswordPolicyScenario()
		{
			if (File.Exists(TargetTestDataNHibFile))
				File.Delete(TargetTestDataNHibFile);
		}

	}
}
