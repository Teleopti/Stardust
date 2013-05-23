using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface IEditRequestPage
	{
		Span AddRequestDropDown { get; }
		Link AddTextRequestMenuItem { get; }
		Div RequestDetailSection { get; }
		Span TextRequestTab { get; }
		Span AbsenceRequestTab { get; }
		TextField AbsenceTypesTextField { get; }
		SelectList AbsenceTypesSelectList { get; }
		CheckBox FulldayCheck { get; }
		TextField RequestDetailSubjectInput { get; }
		TextField RequestDetailFromDateTextField { get; }
		TextField RequestDetailFromTimeTextField { get; }
		TextField RequestDetailToDateTextField { get; }
		TextField RequestDetailToTimeTextField { get; }
		TextField RequestDetailMessageTextField { get; }
		TextField RequestDetailEntityId { get; }
		Div ValidationErrorText { get; }

		Button OkButton { get; set; }
		Element CancelButton { get; }

		Span RequestDetailDenyReason { get; }
	}
}
