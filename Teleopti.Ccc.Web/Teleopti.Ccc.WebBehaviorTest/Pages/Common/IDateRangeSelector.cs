using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface IDateRangeSelector
	{
		DatePicker DatePicker { get; }
		Div DateRangeSelectorContainer { get; }
		void ClickNext();
		void ClickPrevious();
	}
}