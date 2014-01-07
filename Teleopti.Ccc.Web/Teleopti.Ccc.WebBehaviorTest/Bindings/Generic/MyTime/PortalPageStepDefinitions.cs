using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class PortalPageStepDefinitions
	{
		[Then(@"I should see licensed to information")]
		public void ThenIShouldSeeLicensedToInformation()
		{
			var page = Browser.Current.Page<PortalPage>();

			EventualAssert.WhenElementExists(page.LicensedToLabel, c => c.Text, Contains.Substring(UserTexts.Resources.LicensedToColon));
			EventualAssert.WhenElementExists(page.LicensedToText, c => c.Text, Contains.Substring("Teleopti_RD"));
		}
	}
}