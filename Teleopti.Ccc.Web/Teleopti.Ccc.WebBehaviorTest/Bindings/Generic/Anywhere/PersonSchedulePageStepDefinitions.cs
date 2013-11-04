using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
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
			var id = DataMaker.Person(name).Person.Id.ToString();
			Browser.Interactions.AssertUrlContains(id);
			Browser.Interactions.AssertUrlContains(date.Replace("-", ""));
		}

		[Then(@"I should see a scheduled activity with")]
		public void ThenIShouldSeeAScheduledActivityWith(Table table)
		{
			var scheduledActivity = table.CreateInstance<ScheduledActivityInfo>();
			assertScheduledActivity(scheduledActivity);
		}

		[Then(@"I should see these scheduled activities")]
		public void ThenIShouldTheseScheduledActivities(Table table)
		{
			var scheduledActivity = table.CreateSet<ScheduledActivityInfo>();
			scheduledActivity.ForEach(assertScheduledActivity);
		}

		private static void assertScheduledActivity(ScheduledActivityInfo scheduledActivity)
		{
			var selector = string.Format(".shift .layer[data-start-time='{0}'][data-length-minutes='{1}'][style*='background-color: {2}']",
			                             scheduledActivity.StartTime,
			                             scheduledActivity.LengthMinutes(),
			                             ColorNameToCss(scheduledActivity.Color));

			if (scheduledActivity.Description != null)
			{
				DescriptionToggle.EnsureIsOn();
				Browser.Interactions.AssertFirstContains(selector, scheduledActivity.Description);
			}
			else
				Browser.Interactions.AssertExists(selector);
		}

		[Then(@"I should not see a shift layer with")]
		public void ThenIShouldNotSeeAShiftLayerWith(Table table)
		{
			var shiftLayer = table.CreateInstance<ScheduledActivityInfo>();
			Browser.Interactions.AssertNotExists(
				".shift",
				string.Format(".shift .layer[data-start-time='{0}'][data-length-minutes='{1}'][style*='background-color: {2}']",
				              shiftLayer.StartTime,
				              shiftLayer.LengthMinutes(),
				              ColorNameToCss(shiftLayer.Color)));
		}

		public static string ColorNameToCss(string colorName)
		{
			if (colorName.StartsWith("gray"))
				return "gray";
			var color = System.Drawing.Color.FromName(colorName);
			return string.Format("rgb({0}, {1}, {2})", color.R, color.G, color.B);
		}

		[Then(@"I should see a shift")]
		public void ThenIShouldeeAnyShift()
		{
			Browser.Interactions.AssertExists(".shift .layer");
		}

		[Then(@"I should not see any shift")]
		public void ThenIShouldNotSeeAnyShift()
		{
			Browser.Interactions.AssertNotExists(".schedule", ".shift");
		}

		[Then(@"I should see the add full day absence form with")]
		public void ThenIShouldSeeTheAddFullDayAbsenceFormWith(Table table)
		{
			var fullDayAbsenceFormInfo = table.CreateInstance<FullDayAbsenceFormInfo>();

			Browser.Interactions.AssertInputValueUsingJQuery(".full-day-absence-form .start-date", fullDayAbsenceFormInfo.StartDate.ToShortDateString(DataMaker.Data().MyCulture));
			Browser.Interactions.AssertInputValueUsingJQuery(".full-day-absence-form .end-date", fullDayAbsenceFormInfo.StartDate.ToShortDateString(DataMaker.Data().MyCulture));
		}

		[Then(@"I should see the add full day absence form")]
		public void ThenIShouldSeeTheAddFullDayAbsenceForm()
		{
			Browser.Interactions.AssertExists(".full-day-absence-form");
		}

		[When(@"I input these full day absence values")]
		public void WhenIInputTheseFullDayAbsenceValues(Table table)
		{
			var fullDayAbsenceFormInfo = table.CreateInstance<FullDayAbsenceFormInfo>();

			if (!string.IsNullOrEmpty(fullDayAbsenceFormInfo.Absence))
				Browser.Interactions.SelectOptionByTextUsingJQuery(".full-day-absence-form .absence-type", fullDayAbsenceFormInfo.Absence);
			else
				// for robustness. cant understand why this is required. the callViewMethodWhenReady should solve it.
				Browser.Interactions.AssertExists(".full-day-absence-form .absence-type:enabled");

			Browser.Interactions.Javascript(string.Format("test.callViewMethodWhenReady('personschedule', 'setDateFromTest', '{0}');", fullDayAbsenceFormInfo.EndDate));

			Browser.Interactions.AssertInputValueUsingJQuery(".full-day-absence-form .end-date", fullDayAbsenceFormInfo.EndDate.ToShortDateString(DataMaker.Data().MyCulture));
		}

		[Then(@"I should see an absence in the absence list with")]
		public void ThenIShouldSeeAnAbsenceInTheAbsenceListWith(Table table)
		{
			var absenceListItemInfo = table.CreateInstance<AbsenceListItemInfo>();

			Browser.Interactions.AssertExistsUsingJQuery(
				string.Format(".absence-list .absence:contains('{0}'):contains('{1}'):contains('{2}')",
				              absenceListItemInfo.Name,
				              absenceListItemInfo.StartTimeFormatted(),
				              absenceListItemInfo.EndTimeFormatted())
				);
		}

		[Then(@"I should see (.*) absences in the absence list")]
		public void ThenIShouldSeeAbsencesInTheAbsenceList(int count)
		{
			if (count == 0)
				Browser.Interactions.AssertNotExists(".absence-list", ".absence-list .absence");
			else
				Browser.Interactions.AssertNotExists(".absence-list .absence:nth-child(" + count + ")", ".absence-list .absence:nth-child(" + (count + 1) + ")");
		}

		[Then(@"I should see a day off")]
		public void ThenIShouldSeeADayOff()
		{
			Browser.Interactions.AssertExists(".dayoff");
		}

		[Then(@"I should see a day off named '(.*)'")]
		public void ThenIShouldSeeADayOff(string dayOff)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".dayoff:contains('{0}')", dayOff);
		}

		[Then(@"I should see the time line with")]
		public void ThenIShouldSeeTheTimeLineWith(Table table)
		{
			var timeLineInfo = table.CreateInstance<TimeLineInfo>();
			Browser.Interactions.AssertExists(".time-line[data-start-time='{0}'][data-end-time='{1}']", timeLineInfo.StartTime, timeLineInfo.EndTime);
		}

		[When(@"I click '(.*)' on absence named '(.*)'")]
		public void WhenIClickOnAbsenceNamed(CssClass cssClass, string absenceName)
		{
			Browser.Interactions.ClickUsingJQuery(".absence-list .absence:contains('" + absenceName + "') ." + cssClass.Name);
		}

		public class TimeLineInfo
		{
			public string StartTime { get; set; }
			public string EndTime { get; set; }
		}
		
		public class AbsenceListItemInfo
		{
			public string Name { get; set; }
			public string StartTime { get; set; }
			public string EndTime { get; set; }


			public string StartTimeFormatted()
			{
				var format = DataMaker.Me().Culture.DateTimeFormat.ShortDatePattern + " " + DataMaker.Me().Culture.DateTimeFormat.ShortTimePattern;
				return DateTime.Parse(StartTime).ToString(format);
			}

			public string EndTimeFormatted()
			{
				var format = DataMaker.Me().Culture.DateTimeFormat.ShortDatePattern + " " + DataMaker.Me().Culture.DateTimeFormat.ShortTimePattern;
				return DateTime.Parse(EndTime).ToString(format);
			}

		}

		public class ScheduledActivityInfo
		{
			public string StartTime { get; set; }
			public string EndTime { get; set; }
			public string Color { get; set; }
			public string Description { get; set; }

			public int LengthMinutes()
			{
				var result = (int) TimeSpan.Parse(EndTime).Subtract(TimeSpan.Parse(StartTime)).TotalMinutes;
				if (result < 0) result += 60*24;
				return result;
			}
		}

		public class FullDayAbsenceFormInfo
		{
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
			public string Absence { get; set; }
		}
	}
}