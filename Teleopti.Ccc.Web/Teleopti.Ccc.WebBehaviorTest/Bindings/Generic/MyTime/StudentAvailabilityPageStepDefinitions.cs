using System;
using System.Collections.Generic;
using System.Globalization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;

using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class StudentAvailabilityPageStepDefinitions
	{
		[Given(@"I have a student availability with")]
		public void GivenIHaveAStudentAvailabilityWith(Table table)
		{
			var fields = table.CreateInstance<StudentAvailabilityFields>();
			DataMaker.Data().Apply(fields);
		}

		[When(@"I click the close button")]
		public void WhenIClickTheCloseButton()
		{
			Browser.Interactions.Click("#Availability-edit-button");
		}

		[When(@"I select day '(.*)' in student availability")]
		public void WhenISelectDayInStudentAvailability(DateTime date)
		{
			Browser.Interactions.Click(CalendarCells.DateSelector(date));
		}

		[When(@"I select following days in student availability")]
		public void WhenISelectFollowingDaysInStudentAvailability(Table table)
		{
			Browser.Interactions.AssertExists("#init-flag");
			var dates = table.CreateSet<SingleValue>();
			dates.ForEach(date =>
				{
					var selector = CalendarCells.DateSelector(date.Value) + ".ui-selectee";
					Browser.Interactions.AssertExistsUsingJQuery(selector);
					Browser.Interactions.AddClassUsingJQuery("ui-selected", selector);
				});
		}
		
		[When(@"I click the edit button in student availability")]
		public void WhenIClickTheEditButtonInStudentAvailability()
		{
			Browser.Interactions.Click("#Availability-edit-button");
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
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.availability-start-time').timepicker('setTime', '{0}');", fields.StartTime));
			if (fields.EndTime != null)
				Browser.Interactions.Javascript_IsFlaky(string.Format("$('.availability-end-time').timepicker('setTime', '{0}');", fields.EndTime));
			if (fields.NextDay)
				Browser.Interactions.Click(".availability-next-day");
		}

		[When(@"I click the apply student availability button")]
		public void WhenIClickTheApplyStudentAvailabilityButton()
		{
			Browser.Interactions.Click("#Student-availability-apply");
		}

		[Then(@"I should not see the student availability on '(.*)'")]
		public void ThenIShouldNotSeeTheStudentAvailabilityOn(DateTime date)
		{
			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertExistsUsingJQuery(string.Format("{0} .{1}:empty", cell, "day-content-text"));
		}

		[Then(@"I should see the student availability with")]
		[Given(@"I should see the student availability with")]
		public void ThenIShouldSeeTheStudentAvailabilityWith(Table table)
		{
			var fields = table.CreateInstance<StudentAvailabilityFields>();

			var studentAvailabilityForDate = CalendarCells.DateSelector(fields.Date);

			Browser.Interactions.AssertNotVisibleUsingJQuery($"{studentAvailabilityForDate} .loading-small-gray-blue");

			if (fields.StartTime != null)
				Browser.Interactions.AssertFirstContainsUsingJQuery(studentAvailabilityForDate, fields.StartTime);
			if (fields.EndTime != null)
				Browser.Interactions.AssertFirstContainsUsingJQuery(studentAvailabilityForDate, fields.EndTime);
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

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var studentAvailabilityRestriction = new StudentAvailabilityRestriction
			{
				StartTimeLimitation = new StartTimeLimitation(Transform.ToNullableTimeSpan(StartTime), null),
				EndTimeLimitation = new EndTimeLimitation(null, Transform.ToNullableTimeSpan(EndTime))
			};
			var studentAvailabilityDay = new StudentAvailabilityDay(
				person,
				new DateOnly(Date),
				new List<IStudentAvailabilityRestriction>(
					new[]
						{
							studentAvailabilityRestriction
						}));

			var studentAvailabilityRepository = new StudentAvailabilityDayRepository(unitOfWork);
			studentAvailabilityRepository.Add(studentAvailabilityDay);
		}
	}

}