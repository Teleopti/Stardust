using System;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
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
																	 scheduledActivity.StartTimeFormatted(),
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

		[Then(@"I should not see a scheduled activity with")]
		public void ThenIShouldNotSeeAscheduledActivityWith(Table table)
		{
			var scheduledActivity = table.CreateInstance<ScheduledActivityInfo>();
			Browser.Interactions.AssertNotExists(
				".shift",
				string.Format(".shift .layer[data-start-time='{0}'][data-length-minutes='{1}'][style*='background-color: {2}']",
											scheduledActivity.StartTime,
											scheduledActivity.LengthMinutes(),
											ColorNameToCss(scheduledActivity.Color)));
		}

		public static string ColorNameToCss(string colorName)
		{
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
			var form = table.CreateInstance<FullDayAbsenceFormInfo>();
			Browser.Interactions.AssertInputValueUsingJQuery(".full-day-absence-form .start-date", form.StartDate.ToShortDateString(DataMaker.Me().Culture));
			Browser.Interactions.AssertInputValueUsingJQuery(".full-day-absence-form .end-date", form.StartDate.ToShortDateString(DataMaker.Me().Culture));
		}

		[Then(@"I should see the add activity form with")]
		public void ThenIShouldSeeTheAddActivityFormWith(Table table)
		{
			var form = table.CreateInstance<AddActivityFormInfo>();
			Browser.Interactions.AssertInputValueUsingJQuery(".activity-form .start-time", form.StartTime.ToShortTimeString(DataMaker.Me().Culture));
			Browser.Interactions.AssertInputValueUsingJQuery(".activity-form .end-time", form.EndTime.ToShortTimeString(DataMaker.Me().Culture));
		}

		[Then(@"I should see the add intraday absence form with")]
		public void ThenIShouldSeeTheAddIntradayAbsenceFormWith(Table table)
		{
			var form = table.CreateInstance<AddIntradayAbsenceFormInfo>();
			Browser.Interactions.AssertInputValueUsingJQuery(".intraday-absence-form .start-time", form.StartTime.ToShortTimeString(DataMaker.Me().Culture));
			Browser.Interactions.AssertInputValueUsingJQuery(".intraday-absence-form .end-time", form.EndTime.ToShortTimeString(DataMaker.Me().Culture));
		}

		[Then(@"I should see the add full day absence form")]
		public void ThenIShouldSeeTheAddFullDayAbsenceForm()
		{
			Browser.Interactions.AssertExists(".full-day-absence-form");
		}

		[Then(@"I should see the add intraday absence form")]
		public void ThenIShouldSeeTheAddIntradayAbsenceForm()
		{
			Browser.Interactions.AssertExists(".intraday-absence-form");
		}

		[Then(@"I should see the add intraday absence form for '(.*)'")]
		public void ThenIShouldSeeTheAddIntradayAbsenceFormFor(string date)
		{
			Browser.Interactions.AssertUrlContains(date.Replace("-", ""));
		}


		[Then(@"I should see the add activity form")]
		public void ThenIShouldSeeTheAddctivityForm()
		{
			Browser.Interactions.AssertExists(".activity-form");
		}

		[When(@"I input these full day absence values")]
		public void WhenIInputTheseFullDayAbsenceValues(Table table)
		{
			var values = table.CreateInstance<FullDayAbsenceFormInfo>();

			if (!string.IsNullOrEmpty(values.Absence))
				Browser.Interactions.SelectOptionByTextUsingJQuery(".full-day-absence-form .absence-type", values.Absence);
			else
				// for robustness. cant understand why this is required. the callViewMethodWhenReady should solve it.
				Browser.Interactions.AssertExists(".full-day-absence-form .absence-type:enabled");

			Browser.Interactions.Javascript(string.Format("test.callViewMethodWhenReady('personschedule', 'setDateFromTest', '{0}');", values.EndDate));

			Browser.Interactions.AssertInputValueUsingJQuery(".full-day-absence-form .end-date", values.EndDate.ToShortDateString(DataMaker.Me().Culture));
		}

		[When(@"I input these intraday absence values")]
		public void WhenIInputTheseIntradayAbsenceValues(Table table)
		{
			var values = table.CreateInstance<AddIntradayAbsenceFormInfo>();

			Browser.Interactions.SelectOptionByTextUsingJQuery(".intraday-absence-form .absence-type", values.Absence);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".intraday-absence-form .start-time", values.StartTime.ToShortTimeString(DataMaker.Me().Culture));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".intraday-absence-form .end-time", values.EndTime.ToShortTimeString(DataMaker.Me().Culture));
		}

		[When(@"I input these add activity values")]
		public void WhenIInputTheseAddActivityValues(Table table)
		{
			var values = table.CreateInstance<AddActivityFormInfo>();

			Browser.Interactions.SelectOptionByTextUsingJQuery(".activity-form .activity-type", values.Activity);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".activity-form .start-time", values.StartTime.ToShortTimeString(DataMaker.Me().Culture));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".activity-form .end-time", values.EndTime.ToShortTimeString(DataMaker.Me().Culture));
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

		[Then(@"I should see the time line with")]
		[Then(@"I should see my time line with")]
		public void ThenIShouldSeeTheTimeLineWith(Table table)
		{
			var timeLineInfo = table.CreateInstance<TimeLineInfo>();
			Browser.Interactions.AssertExists(".time-line[data-start-time='{0}'][data-end-time='{1}']", timeLineInfo.StartTime, timeLineInfo.EndTime);
		}

		[Then(@"I should see the agent's time line with")]
		public void ThenIShouldSeeTheAgentSTimeLineWith(Table table)
		{
			var timeLineInfo = table.CreateInstance<TimeLineInfo>();
			Browser.Interactions.AssertFirstContains(".other-time-zone-short", timeLineInfo.TimeZone);
			Browser.Interactions.AssertExists(".time-line.other-time-zone[data-start-time='{0}'][data-end-time='{1}']", timeLineInfo.StartTime, timeLineInfo.EndTime);
		}

		[Then(@"I should not see the agent's timeline")]
		public void ThenIShouldNotSeeTheAgentSTimeline()
		{
			Browser.Interactions.AssertNotExists(".time-line", ".time-line.other-time-zone");
		}

		[Then(@"I should not see add activity times in other time zone")]
		public void ThenIShouldNotSeeAddActivityTimesInOtherTimeZone()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".activity-form .other-time-zone span");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".activity-form .other-start-time span");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".activity-form .other-end-time span");
		}

		[Then(@"I should not see add intraday absence times in other time zone")]
		public void ThenIShouldNotSeeAddIntradayAbsenceTimesInOtherTimeZone()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".intraday-absence-form .other-time-zone span");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".intraday-absence-form .other-start-time span");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".intraday-absence-form .other-end-time span");
		}

		[Then(@"I should not see move activity time in other time zone")]
		public void ThenIShouldNotSeeMoveActivityTimeInOtherTimeZone()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".activity-form .other-time-zone span");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".activity-form .other-start-time span");
		}


		[Then(@"I should see add activity times in other time zone with")]
		public void ThenIShouldSeeAddActivityTimesInOtherTimeZoneWith(Table table)
		{
			var localTimeInfo = table.CreateInstance<AddActivityFormInfo>();
			Browser.Interactions.AssertFirstContains(".activity-form .other-start-time span", localTimeInfo.StartTime.ToShortTimeString(DataMaker.Me().Culture));
			Browser.Interactions.AssertFirstContains(".activity-form .other-end-time span", localTimeInfo.EndTime.ToShortTimeString(DataMaker.Me().Culture));
		}

		[Then(@"I should see add intraday absence times in other time zone with")]
		public void ThenIShouldSeeAddIntradayAbsenceTimesInOtherTimeZoneWith(Table table)
		{
			var localTimeInfo = table.CreateInstance<AddIntradayAbsenceFormInfo>();
			Browser.Interactions.AssertFirstContains(".intraday-absence-form .other-start-time span", localTimeInfo.StartTime.ToShortTimeString(DataMaker.Me().Culture));
			Browser.Interactions.AssertFirstContains(".intraday-absence-form .other-end-time span", localTimeInfo.EndTime.ToShortTimeString(DataMaker.Me().Culture));
		}

		[Then(@"I should see selected activity time in other time zone with")]
		public void ThenIShouldSeeSelectedActivityTimeInOtherTimeZoneWith(Table table)
		{
			var localTimeInfo = table.CreateInstance<AddIntradayAbsenceFormInfo>();
			Browser.Interactions.AssertFirstContains(".activity-form .other-start-time span", localTimeInfo.StartTime.ToShortTimeString(DataMaker.Me().Culture));
		}


		[When(@"I click '(.*)' on absence named '(.*)'")]
		public void WhenIClickOnAbsenceNamed(CssClass cssClass, string absenceName)
		{
			Browser.Interactions.ClickUsingJQuery(".absence-list .absence:contains('" + absenceName + "') ." + cssClass.Name);
		}

		[When(@"I select the activity with")]
		public void WhenISelectTheActivityWith(Table table)
		{
			var scheduledActivityInfo = table.CreateInstance<ScheduleActivityInfo>();
			Browser.Interactions.ClickUsingJQuery(".layer[data-start-time='" + scheduledActivityInfo.StartTimeFormatted() + "']");
		}

		[When(@"I view person schedules move activity form for '(.*)' in '(.*)' on '(.*)' with selected start minutes of '(.*)'")]
		public void WhenIViewPersonSchedulesMoveActivityFormForInOnWithSelectedStartTimeOf(string name, string teamName, DateTime date, string startTime)
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			var teamId = (from t in DataMaker.Data().UserDatasOfType<TeamConfigurable>()
										let team = t.Team
										where team.Description.Name.Equals(teamName)
										select team.Id.GetValueOrDefault()).First();
			Navigation.GotoAnywherePersonScheduleMoveActivityForm(personId, teamId, date, startTime);
		}

		[When(@"I move the activity")]
		public void WhenIMoveTheActivity(Table scheduledActivityInfotable)
		{
			var values = scheduledActivityInfotable.CreateInstance<MoveActivityFormInfo>();
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".activity-form .start-time", values.StartTime.ToShortTimeString(DataMaker.Me().Culture));
		}

		[When(@"I move the activity by dnd of '(.*)' minutes")]
		public void WhenIMoveTheActivityByDnd(string minutes)
		{
			var pixelsPerMinute = Browser.Interactions.Javascript("return document.querySelectorAll('.time-line')[0].getAttribute('data-minute-length');");
			Browser.Interactions.DragnDrop(".layer.active", (int)Math.Round(Convert.ToDouble(pixelsPerMinute), 0) * Convert.ToInt32(minutes), 0);
		}

		[When(@"I save the shift")]
		public void WhenISaveTheShift()
		{
			Browser.Interactions.ClickUsingJQuery(".apply");
		}

		[Then(@"I should see the moved schedule activity details for '(.*)' with")]
		public void ThenIShouldSeeTheMovedScheduleActivityDetailsForWith(string personName, Table table)
		{
			var scheduledActivity = table.CreateInstance<ScheduledActivityInfo>();
			assertScheduledActivity(personName, scheduledActivity);
		}


		[Given(@"'(.*)' has a common agent description like '(.*)'")]
		public void GivenHasACommonAgentDescriptionLike(string p0, string p1)
		{
			// TODO
		}

		[Then(@"the displayed name should be '(.*)'")]
		public void ThenTheDisplayedNameShouldBe(string displayedName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(
					".person:contains('{0}') ",
					displayedName);
		}


		private static void assertScheduledActivity(string personName, ScheduledActivityInfo layer)
		{
			if (layer.StartTime.Equals("00:00"))
			{
				// not sure how to assert the length for the night shift starting from yesterday
				Browser.Interactions.AssertExistsUsingJQuery(
					".person:contains('{0}') .shift .layer[style*='background-color: {1}'][style*='left: 0px']",
					personName,
					PersonSchedulePageStepDefinitions.ColorNameToCss(layer.Color)
					);
			}
			else
			{
				Browser.Interactions.AssertExistsUsingJQuery(
					".person:contains('{0}') .shift .layer[data-start-time='{1}'][data-length-minutes='{2}'][style*='background-color: {3}']",
					personName,
					layer.StartTime,
					layer.LengthMinutes(),
					PersonSchedulePageStepDefinitions.ColorNameToCss(layer.Color)
					);
			}
		}

		public class TimeLineInfo
		{
			public string TimeZone { get; set; }
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
			public string Name { get; set; }

			public int LengthMinutes()
			{
				var result = (int)TimeSpan.Parse(EndTime).Subtract(TimeSpan.Parse(StartTime)).TotalMinutes;
				if (result < 0) result += 60 * 24;
				return result;
			}

			public string StartTimeFormatted()
			{
				var format = DataMaker.Me().Culture.DateTimeFormat.ShortTimePattern;
				return DateTime.Parse(StartTime).ToString(format);
			}

			public string EndTimeFormatted()
			{
				var format = DataMaker.Me().Culture.DateTimeFormat.ShortTimePattern;
				return DateTime.Parse(EndTime).ToString(format);
			}
		}

		public class FullDayAbsenceFormInfo
		{
			public string Absence { get; set; }
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
		}

		public class AddIntradayAbsenceFormInfo
		{
			public string Absence { get; set; }
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
		}

		public class AddActivityFormInfo
		{
			public string Activity { get; set; }
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
			public string LocalTimeZone { get; set; }
		}

		public class MoveActivityFormInfo
		{
			public DateTime StartTime { get; set; }
		}

	}
}