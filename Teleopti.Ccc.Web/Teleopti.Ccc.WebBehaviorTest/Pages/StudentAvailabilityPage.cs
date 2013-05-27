using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class StudentAvailabilityPage : CalendarCellsPage, IDateRangeSelector, IDeleteButton
	{
		[FindBy(Id = "Availability-edit-button")]
		public Button EditButton;
		[FindBy(Id = "Availability-delete-button")]
		public Button DeleteButton { get; set; }

		[FindBy(Id = "qtip-edit-student-availability")]
		public Div EditStudentAvailabilityPanel;
		[FindBy(Id = "Student-availability-start-time")]
		public ComboBox StudentAvailabilityStartTime;
		[FindBy(Id = "Student-availability-end-time")]
		public ComboBox StudentAvailabilityEndTime;
		[FindBy(Id = "Student-availability-end-time-next-day")]
		public CheckBox StudentAvailabilityEndTimeNextDay;
		[FindBy(Id = "Student-availability-apply")]
		public Button StudentAvailabilityApplyButton { get; set; }

		[FindBy(Id = "StudentAvailability-period")]
		public Div StudentAvailabilityPeriod;

		[FindBy(Id = "Student-availability-apply-error")]
		public Div ValidationError;

		[FindBy(Id = "AvailabilityDateRangeSelector")] public Div DateRangeSelectorContainer { get; set; }
		[FindBy(Id = "StudentAvailabilityScheduleDatePicker")] public DatePicker DatePicker { get; set; }
		public Button NextPeriodButton { get { return DateRangeSelectorContainer.Buttons.Last(); } }
		public Button PreviousPeriodButton { get { return DateRangeSelectorContainer.Buttons.First(); } }


		public void ClickNext()
		{
			Browser.Interactions.Click("#AvailabilityDateRangeSelector button:last-of-type");
		}

		public void ClickPrevious()
		{
			Browser.Interactions.Click("#AvailabilityDateRangeSelector button:first-of-type");
		}

	}
}