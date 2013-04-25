using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class StudentAvailabilityPage : CalendarCellsPage, IDateRangeSelector, IDeleteButton
	{
		[FindBy(Id = "StudentAvailability-edit-button")]
		public Button EditButton;
		[FindBy(Id = "StudentAvailability-delete-button")]
		public Button DeleteButton { get; set; }

		//[FindBy(Id = "StudentAvailability-edit-starttime-input")]
		//public TextField StartTimeTextField;
		//[FindBy(Id = "StudentAvailability-edit-endtime-input")]
		//public TextField EndTimeTextField;
		//[FindBy(Id = "StudentAvailability-edit-nextday-cb")]
		//public CheckBox NextDay;

		//[FindBy(Id = "StudentAvailability-edit-ok-button")]
		//public Button OkButton { get; set; }

		//[FindBy(Id = "StudentAvailability-edit-cancel-button")]
		//public Button CancelButton { get; set; }

		//[FindBy(Id = "StudentAvailability-edit-section")]
		//public Div InputPanel { get; set; }

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

		[FindBy(Id = "StudentAvailabilityDateRangeSelector")] public Div DateRangeSelectorContainer { get; set; }
		[FindBy(Id = "StudentAvailabilityScheduleDatePicker")] public DatePicker DatePicker { get; set; }
		public Button NextPeriodButton { get { return DateRangeSelectorContainer.Buttons.Last(); } }
		public Button PreviousPeriodButton { get { return DateRangeSelectorContainer.Buttons.First(); } }


		public void ClickNext()
		{
			Browser.Interactions.Click("#StudentAvailabilityDateRangeSelector button:last-of-type");
		}

		public void ClickPrevious()
		{
			Browser.Interactions.Click("#StudentAvailabilityDateRangeSelector button:first-of-type");
		}

	}
}