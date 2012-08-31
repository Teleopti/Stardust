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
		private static readonly Uri asmUri = new Uri(TestSiteConfigurationSetup.Url,"MyTime/Asm");

		[When(@"I click ASM link")]
		public void WhenIClickASMLink()
		{
			Pages.Pages.CurrentPortalPage.AsmButton.Click();
		}

		[Then(@"I should see a schedule in popup")]
		public void ThenIShouldSeeAScheduleInPopup()
		{
			using (var asmPopup = Browser.AttachTo<IE>(Find.ByUrl(asmUri)))
			{
				var layers = asmPopup.Spans.Filter(Find.ByClass("asm-layer",false));
				EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
			}
		}

		[Then(@"I should see the name of current activity")]
		public void ThenIShouldSeeTheNameOfCurrentActivity()
		{
			using (var asmPopup = Browser.AttachTo<IE>(Find.ByUrl(asmUri)))
			{
				var element = asmPopup.Element(Find.ById("asm-info-current-activity"));
				EventualAssert.That(() => element.Text, Is.EqualTo("lunch"));
			}
		}
	}
}
