using System;
using System.Linq;
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
		private const string attributeUsedForWidth = "padding-left";
		public static readonly Uri asmUri = new Uri(TestSiteConfigurationSetup.Url, "MyTime/Asm");
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
			EventualAssert.That(() => _popup.Spans.Filter(Find.ByClass("asm-layer", false)).Count, Is.GreaterThan(0));
		}

		[Then(@"I should see '(.*)' upcoming activities")]
		public void ThenIShouldSeeUpcomingActivities(int numberOfUpcomingActivities)
		{
			EventualAssert.That(() => _popup.Table("asm-current-info-table").TableRows.Count, Is.EqualTo(numberOfUpcomingActivities));
		}


		[Then(@"I should see Phone as current activity")]
		public void ThenIShouldSeePhoneAsCurrentActivity()
		{
			EventualAssert.That(() =>
				_popup.Table("asm-current-info-table").TableRow(Find.ByClass("asm-info-current-activity")).Children().Filter(Find.ByText(TestData.ActivityPhone.Description.Name)).Count,
				Is.EqualTo(1));
		}

		[Then(@"I should not see as current activity")]
		public void ThenIShouldNotSeeAsCurrentActivity()
		{
			EventualAssert.That(() => _popup.Table("asm-current-info-table").TableRow(Find.ByClass("asm-info-current-activity")).Exists, Is.False);
		}

		[Then(@"ASM link should not be visible")]
		public void ThenASMLinkShouldNotBeVisible()
		{
			EventualAssert.That(() => Pages.Pages.CurrentPortalPage.AsmButton.Exists, Is.False);
		}

		[Then(@"The last layer should be '(.*)' hours long")]
		public void ThenTheLastLayerShouldBeHoursLong(int hours)
		{
			EventualAssert.That(() =>
										{
											var allLayers = _popup.Elements.Filter(Find.ByClass("asm-layer", false));
											var oneHourLayer = allLayers.First();
											var pxPerHour = pixelLength(oneHourLayer);
											var theLayerToCheck = allLayers.Last();
											return pixelLength(theLayerToCheck) / pxPerHour;
										}, Is.EqualTo(hours));
		}

		[Then(@"I should see last activity starttime as '(.*)'")]
		public void ThenIShouldSeeLastActivityStarttimeAs(string startTime)
		{
			EventualAssert.That(() =>
									  _popup.Table("asm-current-info-table")
										  .Elements.Filter(Find.ByClass("asm-info-time-column"))
										  .Last().Text,
										Is.EqualTo(startTime));
		}

		private static int pixelLength(Element oneHourLengthLayer)
		{
			return Convert.ToInt32(oneHourLengthLayer.Style.GetAttributeValue(attributeUsedForWidth).TrimEnd('p', 'x'));
		}


		[AfterScenario("ASM")]
		[AfterScenario("ASMWinterSummer")]
		[AfterScenario("ASMSummerWinter")]
		public void AfterScenario()
		{
			killPopupIfExists();
		}

		private void killPopupIfExists()
		{
			if (_popup != null)
				_popup.Dispose();
		}
	}
}
