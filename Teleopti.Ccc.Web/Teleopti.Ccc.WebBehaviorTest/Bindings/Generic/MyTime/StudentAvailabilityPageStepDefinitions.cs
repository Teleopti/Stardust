using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class StudentAvailabilityPageStepDefinitions
	{
		public static string Cell(DateTime date)
		{
			return string.Format("li[data-mytime-date='{0}']", date.ToString("yyyy-MM-dd"));
		}

		public static void SelectCalendarCellByClass(DateTime date)
		{
			Browser.Interactions.AssertExists(Cell(date) + ".ui-selectee");
			Browser.Interactions.AddClassUsingJQuery("ui-selected", Cell(date) + ".ui-selectee");
		}

		[Given(@"I have a student availability with")]
		public void GivenIHaveAStudentAvailabilityWith(Table table)
		{
			var fields = table.CreateInstance<StudentAvailabilityFields>();
			DataMaker.Data().Apply(fields);
		}

		[When(@"I click the close button")]
		public void WhenIClickTheCloseButton()
		{
			Pages.Pages.StudentAvailabilityPage.EditStudentAvailabilityPanel.Link(Find.ByClass("ui-state-default qtip-close qtip-icon"));
		}

		[When(@"I select day '(.*)' in student availability")]
		public void WhenISelectDayInStudentAvailability(DateTime date)
		{
			SelectCalendarCellByClass(date);
		}

		[When(@"I select following days in student availability")]
		public void WhenISelectFollowingDaysInStudentAvailability(Table table)
		{
			var dates = table.CreateSet<SingleValue>();
			dates.ForEach(date => SelectCalendarCellByClass(date.Value));
		}
		
		[When(@"I click the edit button in student availability")]
		public void WhenIClickTheEditButtonInStudentAvailability()
		{
			Browser.Interactions.Javascript("$('#Availability-edit-button').mousedown();");
		}

		[When(@"I click the delete button in student availability")]
		public void WhenIClickTheDeleteButtonInStudentAvailability()
		{
			Browser.Interactions.Click("#Availability-delete-button");
		}

		[When(@"I input student availability with")]
		public void WhenIInputStudentAvailabilityWith(Table table)
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Student-availability-edit-form"); //needed

			var fields = table.CreateInstance<StudentAvailabilityFields>();

			if (fields.StartTime != null)
				Browser.Interactions.Javascript(string.Format("$('.availability-start-time').timepicker('setTime', '{0}');", fields.StartTime));
			if (fields.EndTime != null)
				Browser.Interactions.Javascript(string.Format("$('.availability-end-time').timepicker('setTime', '{0}');", fields.EndTime));
			if (fields.NextDay)
				Browser.Interactions.Click(".availability-next-day");
		}

		[When(@"I click the apply student availability button")]
		public void WhenIClickTheApplyStudentAvailabilityButton()
		{
			Pages.Pages.StudentAvailabilityPage.StudentAvailabilityApplyButton.EventualClick();
		}

		[Then(@"I should not see the student availability on '(.*)'")]
		public void ThenIShouldNotSeeTheStudentAvailabilityOn(DateTime date)
		{
			var cell = Pages.Pages.StudentAvailabilityPage.CalendarCellForDate(date);
			Assert.That(() => cell.Child(QuicklyFind.ByClass("day-content")).Text, Is.Null.After(5000, 10));
		}

		[Then(@"I should see the student availability with")]
		public void ThenIShouldSeeTheStudentAvailabilityWith(Table table)
		{
			var fields = table.CreateInstance<StudentAvailabilityFields>();

			var studentAvailabilityForDate = Pages.Pages.StudentAvailabilityPage.CalendarCellForDate(fields.Date);

			if (fields.StartTime != null) EventualAssert.That(() => studentAvailabilityForDate.InnerHtml, Is.StringContaining(fields.StartTime));
			if (fields.EndTime != null) EventualAssert.That(() => studentAvailabilityForDate.InnerHtml, Is.StringContaining(fields.EndTime));
		}

		[Then(@"I should see a message '(.*)'")]
		public void ThenIShouldSeeAMessage(string message)
		{
			Browser.Interactions.AssertAnyContains(".availability-error-panel", string.Format(Resources.InvalidTimeValue, Resources.EndTime));
		}

		private class SingleValue
		{
			public DateTime Value { get; set; }
		}
	}

	public class StudentAvailabilityFields : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool NextDay { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(Transform.ToNullableTimeSpan(StartTime), null),
				EndTimeLimitation = new EndTimeLimitation(null, Transform.ToNullableTimeSpan(EndTime))
			};
			var studentAvailabilityDay = new StudentAvailabilityDay(
				user,
				new DateOnly(Date),
				new List<IStudentAvailabilityRestriction>(
					new[]
						{
							studentAvailabilityRestriction
						}));

			var studentAvailabilityRepository = new StudentAvailabilityDayRepository(uow);
			studentAvailabilityRepository.Add(studentAvailabilityDay);
		}
	}

}