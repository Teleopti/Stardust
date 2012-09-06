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
		public static readonly Uri asmUri = new Uri(TestSiteConfigurationSetup.Url,"MyTime/Asm");
		private IE _popup;

		[When(@"I click ASM link")]
		public void WhenIClickASMLink()
		{
			Pages.Pages.CurrentPortalPage.AsmButton.Click();
			_popup = Browser.AttachTo<IE>(Find.ByUrl(asmUri));
		}

		[Then(@"I should see a schedule in popup")]
		public void ThenIShouldSeeAScheduleInPopup()
		{
			var layers = _popup.Spans.Filter(Find.ByClass("asm-layer",false));
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
		}

		[Then(@"I should see '(.*)' upcoming activities")]
		public void ThenIShouldSeeUpcomingActivities(int numberOfUpcomingActivities)
		{
			EventualAssert.That(() => _popup.Table("asm-current-info-table").TableRows.Count, Is.EqualTo(numberOfUpcomingActivities));
		}


		[Then(@"I should see Phone as current activity")]
		public void ThenIShouldSeePhoneAsCurrentActivity()
		{
			var infoTable = _popup.Table("asm-current-info-table");
			var activeTdsWithPhone = infoTable.TableRow(Find.ByClass("asm-info-current-activity")).Children().Filter(Find.ByText(TestData.ActivityPhone.Description.Name));
			EventualAssert.That(() => activeTdsWithPhone.Count, Is.EqualTo(1));
		}

		[Then(@"I should not see as current activity")]
		public void ThenIShouldNotSeeAsCurrentActivity()
		{
			var infoTable = _popup.Table("asm-current-info-table");
			var activeTd = infoTable.TableRow(Find.ByClass("asm-info-current-activity"));
			EventualAssert.That(() => activeTd.Exists, Is.False);
		}

		[Then(@"ASM link should not be visible")]
		public void ThenASMLinkShouldNotBeVisible()
		{
			EventualAssert.That(()=>Pages.Pages.CurrentPortalPage.AsmButton.Exists,Is.False);
		}

		[AfterScenario("ASM")]
		public void AfterScenario()
		{
			killPopupIfExists();
		}

		private void killPopupIfExists()
		{
			if(_popup!=null)
				_popup.Dispose();
		}
	}
}
