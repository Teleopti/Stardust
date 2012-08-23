using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = WatiN.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AsmStepDefinition
	{
		[When(@"I click ASM link")]
		public void WhenIClickASMLink()
		{
			Pages.Pages.CurrentPortalPage.AsmButton.Click();
		}

		[Then(@"I should see a schedule in popup")]
		public void ThenIShouldSeeAScheduleInPopup()
		{
			var uri =new Uri(TestSiteConfigurationSetup.Url,"MyTime/Asm");
			var asmPopup = Browser.AttachTo<IE>(Find.ByUrl(uri));
			var layers = asmPopup.Divs.Filter(Find.ByClass("asm-layer",false));
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));

		}
	}
}
