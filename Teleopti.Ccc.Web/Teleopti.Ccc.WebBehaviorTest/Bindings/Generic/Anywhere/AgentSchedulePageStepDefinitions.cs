using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
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
		[Then(@"I should see agent schedule for '(.*)' on '(.*)'")]
		public void ThenIShouldSeeAgentScheduleForAgentOnDate(string name, string date)
		{
			var id = UserFactory.User(name).Person.Id.ToString();
			EventualAssert.That(() => Browser.Current.Url.Contains(id), Is.True);
			EventualAssert.That(() => Browser.Current.Url.Contains(date.Replace("-", "")), Is.True);
		}

		[Then(@"I should see a shift")]
		public void ThenIShouldSeeAShift()
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift .layer")).Exists, Is.True);
		}

		[Then(@"I should see (.*) shift layers")]
		public void ThenIShouldSeeNumberOfShiftLayers(int numberOfShifts)
		{
			EventualAssert.That(() => Browser.Current.Elements.Filter(Find.BySelector(".shift .layer")).Count, Is.EqualTo(numberOfShifts));
		}

		[Then(@"I should not see any shift")]
		public void ThenIShouldNotSeeAnyShift()
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift")).Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift .layer")).Exists, Is.False);
		}

		[Then(@"I should see a shift layer with")]
		public void ThenIShouldSeeAShiftLayerWith(Table table)
		{
			var shiftLayer = table.CreateInstance<ShiftLayerInfo>();
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift .layer")).GetAttributeValue("data-start-time"), Is.EqualTo(shiftLayer.StartTime));
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift .layer:last")).GetAttributeValue("data-end-time"), Is.EqualTo(shiftLayer.EndTime));
		}

		//[When(@"I click (.*)")]
		//public void WhenIClick(string buttonText)
		//{
		//    Browser.Current.Element(Find.BySelector(".addFullDayAbsence")).EventualClick();
		//}
		
		[When(@"I click add full day absence")]
		public void WhenIClickAddFullDayAbsence()
		{
			Browser.Current.Element(Find.BySelector(".addFullDayAbsence")).EventualClick();
		}

		[Then(@"I should see the add full day absence form with")]
		public void ThenIShouldSeeTheAddFullDayAbsenceFormWith(Table table)
		{
			var fullDayAbsenceFormInfo = table.CreateInstance<FullDayAbsenceFormInfo>();
			EventualAssert.That(() => DateTime.Parse(Browser.Current.Element(Find.BySelector(".full-day-absence .start-date")).Text), Is.EqualTo(fullDayAbsenceFormInfo.StartDate));
			EventualAssert.That(() => DateTime.Parse(Browser.Current.Element(Find.BySelector(".full-day-absence .end-date")).GetAttributeValue("value")), Is.EqualTo(fullDayAbsenceFormInfo.EndDate));
		}

		[When(@"I input these full day absence values")]
		public void WhenIInputTheseFullDayAbsenceValues(Table table)
		{
			var fullDayAbsenceFormInfo = table.CreateInstance<FullDayAbsenceFormInfo>();
			Browser.Current.SelectList(Find.BySelector(".full-day-absence .absence-type")).Option(fullDayAbsenceFormInfo.Absence).Select(); //Click?? or SelectNoWait?
			Browser.Current.Element(Find.BySelector(".full-day-absence .end-date")).SetAttributeValue("value", fullDayAbsenceFormInfo.EndDate.ToString("yyyy-MM-dd"));
		}

		public class ShiftLayerInfo
		{
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
		}

		public class FullDayAbsenceFormInfo
		{
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
			public string Absence { get; set; }
		}
	}
}