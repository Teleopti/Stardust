using System;
using System.Globalization;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class PersonSchedulePageStepDefinitions
	{
		[Then(@"I should be viewing person schedule for '(.*)' on '(.*)'")]
		public void ThenIShouldSeePersonScheduleForPersonOnDate(string name, string date)
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
			var minutes = TimeSpan.Parse(shiftLayer.EndTime).Subtract(TimeSpan.Parse(shiftLayer.StartTime)).TotalMinutes;
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".shift .layer[data-start-time='{0}']", shiftLayer.StartTime))).Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".shift .layer[data-length-minutes='{0}']", minutes))).Exists, Is.True);
			if (shiftLayer.Color != null)
				EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".shift .layer[data-start-time='{0}']", shiftLayer.StartTime))).Style.BackgroundColor.ToName, Is.EqualTo(shiftLayer.Color));
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

			EventualAssert.That(() => DateTime.Parse(Browser.Current.Element(Find.BySelector(".full-day-absence-form .start-date")).GetAttributeValue("value")), Is.EqualTo(fullDayAbsenceFormInfo.StartDate));
			EventualAssert.That(() => DateTime.Parse(Browser.Current.Element(Find.BySelector(".full-day-absence-form .end-date")).GetAttributeValue("value")), Is.EqualTo(fullDayAbsenceFormInfo.EndDate));
		}

		[Then(@"I should see the add full day absence form")]
		public void ThenIShouldSeeTheAddFullDayAbsenceForm()
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".full-day-absence-form")).Exists, Is.True);
		}

		[When(@"I input these full day absence values")]
		public void WhenIInputTheseFullDayAbsenceValues(Table table)
		{
			var fullDayAbsenceFormInfo = table.CreateInstance<FullDayAbsenceFormInfo>();

			var select = Browser.Current.SelectList(Find.BySelector(".full-day-absence-form .absence-type"));
			if (fullDayAbsenceFormInfo.Absence != null)
				select.SelectNoWait(fullDayAbsenceFormInfo.Absence);
			else
				select.WaitUntilEnabled();

			Retrying.Javascript(string.Format("test.callViewMethodWhenReady('personschedule', 'setDateFromTest', '{0}');", fullDayAbsenceFormInfo.EndDate));
		}

		[Then(@"I should see an absence in the absence list with")]
		public void ThenIShouldSeeAnAbsenceInTheAbsenceListWith(Table table)
		{
			var absenceListItemInfo = table.CreateInstance<AbsenceListItemInfo>();

			var selector = string.Format(".absence-list .absence:contains('{0}'):contains('{1}'):contains('{2}')", 
				absenceListItemInfo.Name, absenceListItemInfo.StartTime, absenceListItemInfo.EndTime);

			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(selector)).Exists, Is.True);
		}

		[Then(@"I should see (.*) absences in the absence list")]
		public void ThenIShouldSeeAbsencesInTheAbsenceList(int numberOf)
		{
			EventualAssert.That(() => Browser.Current.Elements.Filter(Find.BySelector(".absence-list .absence")).Count, Is.EqualTo(numberOf));
		}

		[When(@"I click '(.*)' on absence named '(.*)'")]
		public void WhenIClickOnAbsenceNamed(string className, string absenceName)
		{
			className = className.ToLower().Replace(" ", "-");
			Browser.Current.Elements.Filter(
				Find.BySelector(".absence-list .absence:contains('" + absenceName + "')")).Filter(
				Find.BySelector("." + className)).First().EventualClick();
		}
		
		public class AbsenceListItemInfo
		{
			public string Name { get; set; }
			public string StartTime { get; set; }
			public string EndTime { get; set; }
		}

		public class ShiftLayerInfo
		{
			public string StartTime { get; set; }
			public string EndTime { get; set; }
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