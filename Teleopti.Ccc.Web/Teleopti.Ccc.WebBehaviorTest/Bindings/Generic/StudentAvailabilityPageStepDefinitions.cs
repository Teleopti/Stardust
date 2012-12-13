using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using WatiN.Core;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class StudentAvailabilityPageStepDefinitions
	{
		[When(@"I select day '(.*)' in student availability")]
		public void WhenISelectDayInStudentAvailability(DateTime date)
		{
			Pages.Pages.StudentAvailabilityPage.SelectCalendarCellByClass(date);
		}

		[When(@"I select following days in student availability")]
		public void WhenISelectFollowingDaysInStudentAvailability(Table table)
		{
			var dates = table.CreateSet<SingleValue>();
			dates.ForEach(date => Pages.Pages.StudentAvailabilityPage.SelectCalendarCellByClass(date.Value));
		}

		private class SingleValue
		{
			public DateTime Value { get; set; }
		}


		[When(@"I click the edit button in student availability")]
		public void WhenIClickTheEditButtonInStudentAvailability()
		{
			Pages.Pages.StudentAvailabilityPage.EditButton.Focus();
			Pages.Pages.StudentAvailabilityPage.EditButton.EventualClick();
		}

		[When(@"I input student availability with")]
		public void WhenIInputStudentAvailabilityWith(Table table)
		{
			var fields = table.CreateInstance<StudentAvailabilityFields>();
			Pages.Pages.StudentAvailabilityPage.EditStudentAvailabilityPanel.WaitUntilDisplayed(); //needed

			if (fields.StartTime != null)
				Pages.Pages.StudentAvailabilityPage.StudentAvailabilityStartTime.Value = fields.StartTime;
			if (fields.EndTime != null)
				Pages.Pages.StudentAvailabilityPage.StudentAvailabilityEndTime.Value = fields.EndTime;
		}

		[When(@"I click the apply student availability button")]
		public void WhenIClickTheApplyStudentAvailabilityButton()
		{
			Pages.Pages.StudentAvailabilityPage.StudentAvailabilityApplyButton.EventualClick();
		}

		[Then(@"I should see the student availability with")]
		public void ThenIShouldSeeTheStudentAvailabilityWith(Table table)
		{
			var fields = table.CreateInstance<StudentAvailabilityFields>();

			var studentAvailabilityForDate = Pages.Pages.StudentAvailabilityPage.CalendarCellForDate(fields.Date);

			if (fields.StartTime != null) EventualAssert.That(() => studentAvailabilityForDate.InnerHtml, Is.StringContaining(fields.StartTime));
			if (fields.EndTime != null) EventualAssert.That(() => studentAvailabilityForDate.InnerHtml, Is.StringContaining(fields.EndTime));
		}
	}

	public class StudentAvailabilityFields
	{
		public DateTime Date { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool EndTimeNextDay { get; set; }
	}

}