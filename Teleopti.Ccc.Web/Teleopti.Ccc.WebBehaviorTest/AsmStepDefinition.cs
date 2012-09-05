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

		[Then(@"I should see Phone as current activity")]
		public void ThenIShouldSeePhoneAsCurrentActivity()
		{
			var element = _popup.Element(Find.ById("asm-info-current-activity"));
			EventualAssert.That(() => element.Text, Is.EqualTo(TestData.ActivityPhone.Description.Name));
		}


		[Then(@"I should see Lunch as next activity")]
		public void ThenIShouldSeeLunchAsNextActivity()
		{
			var element = _popup.Element(Find.ById("asm-info-next-activity"));
			EventualAssert.That(() => element.Text, Is.EqualTo(TestData.ActivityLunch.Description.Name));
		}

		[Then(@"I should see Phone as next activity")]
		public void ThenIShouldSeePhoneAsNextActivity()
		{
			var element = _popup.Element(Find.ById("asm-info-next-activity"));
			EventualAssert.That(() => element.Text, Is.EqualTo(TestData.ActivityPhone.Description.Name));
		}

		[Then(@"I should see '(.*)' as current end time")]
		public void ThenIShouldSeeTimeAsCurrentEndTime(string time)
		{
			var element = _popup.Element(Find.ById("asm-info-current-endtime"));
			var stringToCompare = string.IsNullOrEmpty(time) ? null : time;
			EventualAssert.That(() => element.Text, Is.EqualTo(stringToCompare));
		}

		[Then(@"I should see '(.*)' as current start time")]
		public void ThenIShouldSeeTimeAsCurrentStartTime(string time)
		{
			var element = _popup.Element(Find.ById("asm-info-current-starttime"));
			var stringToCompare = string.IsNullOrEmpty(time) ? null : time;
			EventualAssert.That(() => element.Text, Is.EqualTo(stringToCompare));
		}

		[Then(@"I should see '(.*)' as next end time")]
		public void ThenIShouldSeeTimeAsNextEndTime(string time)
		{
			var element = _popup.Element(Find.ById("asm-info-next-endtime"));
			var stringToCompare = string.IsNullOrEmpty(time) ? null : time;
			EventualAssert.That(() => element.Text, Is.EqualTo(stringToCompare));
		}

		[Then(@"I should see '(.*)' as next start time")]
		public void ThenIShouldSeeTimeAsNextStartTime(string time)
		{
			var element = _popup.Element(Find.ById("asm-info-next-starttime"));
			var stringToCompare = string.IsNullOrEmpty(time) ? null : time;
			EventualAssert.That(() => element.Text, Is.EqualTo(stringToCompare));
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
