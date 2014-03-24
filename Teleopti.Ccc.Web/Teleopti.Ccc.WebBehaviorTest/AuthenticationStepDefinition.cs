using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AuthenticationStepDefinition
	{
		private static readonly string TargetTestDataNHibFile = Path.Combine(Paths.WebBinPath(), "TestData2.nhib.xml");

		[Given(@"I have access to two data sources")]
		public void GivenIHaveAccessToTwoDataSources()
		{
			FileConfigurator.ConfigureByTags(
				"Data\\TestData2.nhib.xml",
				TargetTestDataNHibFile,
				new AllTags()
				);
		}

		[When(@"I select one data source")]
		public void WhenISelectOneDataSource()
		{
			// first one is selected as default
		}
	}
}
