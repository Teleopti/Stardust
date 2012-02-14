using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface IAddTextRequestPage
	{
		Button AddTextRequestButton { get; }
		Div AddTextRequestForm { get; }
		TextField TextRequestSubjectTextField { get; }
		TextField TextRequestFromDateTextField { get; }
		TextField TextRequestFromTimeTextField { get; }
		TextField TextRequestToDateTextField { get; }
		TextField TextRequestToTimeTextField { get; }
		TextField TextRequestMessafeTextField { get; }
		Div ValidationErrorText { get; }

		Button OkButton { get; set; }
		Button CancelButton { get; set; }
	}
}
