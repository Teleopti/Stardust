using System;
using System.Globalization;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class AgentSchedulePageStepDefinitions
	{
		[Then(@"I should be viewing agent schedule for '(.*)' on '(.*)'")]
		public void ThenIShouldSeeAgentScheduleForAgentOnDate(string name, string date)
		{
			var id = UserFactory.User(name).Person.Id.ToString();
			EventualAssert.That(() => Browser.Current.Url.Contains(id), Is.True);
			EventualAssert.That(() => Browser.Current.Url.Contains(date.Replace("-", "")), Is.True);
		}

		[Then(@"I should see these shift layers")]
		public void ThenIShouldSeeTheseShiftLayers(Table table)
		{
			var shiftLayers = table.CreateSet<ShiftLayerInfo>();
			shiftLayers.ForEach(AssertShiftLayer);
		}

		[Then(@"I should see a shift layer with")]
		public void ThenIShouldSeeAShiftLayerWith(Table table)
		{
			var shiftLayer = table.CreateInstance<ShiftLayerInfo>();
			AssertShiftLayer(shiftLayer);
		}

		private static void AssertShiftLayer(ShiftLayerInfo shiftLayer)
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".shift .layer[data-start-time='{0}']", shiftLayer.StartTime))).Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".shift .layer[data-end-time='{0}']", shiftLayer.EndTime))).Exists, Is.True);
			if (shiftLayer.Color != null)
				EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".shift .layer[data-start-time='{0}']", shiftLayer.StartTime))).Style.Color, Is.EqualTo(shiftLayer.Color));
		}

		[Then(@"I should not see any shift")]
		public void ThenIShouldNotSeeAnyShift()
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift")).Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift .layer")).Exists, Is.False);
		}

		[Then(@"I should see the add full day absence form with")]
		public void ThenIShouldSeeTheAddFullDayAbsenceFormWith(Table table)
		{
			var fullDayAbsenceFormInfo = table.CreateInstance<FullDayAbsenceFormInfo>();
			EventualAssert.That(() => DateTime.Parse(Browser.Current.Element(Find.BySelector(".full-day-absence .start-date")).Text), Is.EqualTo(fullDayAbsenceFormInfo.StartDate));
			EventualAssert.That(() => DateTime.Parse(Browser.Current.Element(Find.BySelector(".full-day-absence .end-date")).GetAttributeValue("value")), Is.EqualTo(fullDayAbsenceFormInfo.EndDate));
		}

		[Then(@"I should see the add full day absence form")]
		public void ThenIShouldSeeTheAddFullDayAbsenceForm()
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".full-day-absence")).Exists, Is.True);
		}

		[When(@"I input these full day absence values")]
		public void WhenIInputTheseFullDayAbsenceValues(Table table)
		{
			var fullDayAbsenceFormInfo = table.CreateInstance<FullDayAbsenceFormInfo>();
			if (fullDayAbsenceFormInfo.Absence != null)
			{
				Browser.Current.SelectList(Find.BySelector(".full-day-absence .absence-type")).SelectByValue(fullDayAbsenceFormInfo.Absence);
			}
			Browser.Current.TextField(Find.BySelector(".full-day-absence .end-date")).Value = fullDayAbsenceFormInfo.EndDate.ToShortDateString(UserFactory.User().Culture);
		}

		public class ShiftLayerInfo
		{
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
			public string Color { get; set; }
		}

		public class FullDayAbsenceFormInfo
		{
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
			public string Absence { get; set; }
		}
	}
}