using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface IEditRequestPage
	{
		Button AddTextRequestButton { get; }
		Div RequestDetailSection { get; }
		Span AbsenceRequestTab { get; }
		TextField AbsenceTypesTextField { get; }
		SelectList AbsenceTypesSelectList { get; }
		CheckBox FulldayCheck { get; }
		TextField TextRequestDetailSubjectInput { get; }
		TextField TextRequestDetailFromDateTextField { get; }
		TextField TextRequestDetailFromTimeTextField { get; }
		TextField TextRequestDetailToDateTextField { get; }
		TextField TextRequestDetailToTimeTextField { get; }
		TextField TextRequestDetailMessageTextField { get; }
		TextField TextRequestDetailEntityId { get; }
		Div ValidationErrorText { get; }

		Button OkButton { get; set; }
		Element CancelButton { get; }
	}
}
