using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class StudentAvailabilityPage : CalendarCellsPage, IDateRangeSelector, IDeleteButton, IOkButton
	{
		[FindBy(Id = "StudentAvailability-edit-button")]
		public Button EditButton;

		[FindBy(Id = "StudentAvailability-edit-starttime-input")]
		public TextField StartTimeTextField;
		[FindBy(Id = "StudentAvailability-edit-endtime-input")]
		public TextField EndTimeTextField;
		[FindBy(Id = "StudentAvailability-edit-nextday-cb")]
		public CheckBox NextDay;

		[FindBy(Id = "StudentAvailability-edit-ok-button")]
		public Button OkButton { get; set; }

		[FindBy(Id = "StudentAvailability-edit-cancel-button")]
		public Button CancelButton;

		[FindBy(Id = "StudentAvailability-edit-section")]
		public Div InputPanel;

		[FindBy(Id = "StudentAvailability-period")]
		public Div StudentAvailabilityPeriod;

		[FindBy(Id = "StudentAvailability-edit-error")]
		public Div ValidationErrorText;

		[FindBy(Id = "StudentAvailability-delete-button")]
		public Button DeleteButton { get; set; }

		[FindBy(Id = "StudentAvailabilityDateRangeSelector")] public Div DateRangeSelectorContainer { get; set; }
		[FindBy(Id = "StudentAvailabilityScheduleDatePicker")] public DatePicker DatePicker { get; set; }
		public Button NextPeriodButton { get { return DateRangeSelectorContainer.Buttons.Last(); } }
		public Button PreviousPeriodButton { get { return DateRangeSelectorContainer.Buttons.First(); } }

	}
}