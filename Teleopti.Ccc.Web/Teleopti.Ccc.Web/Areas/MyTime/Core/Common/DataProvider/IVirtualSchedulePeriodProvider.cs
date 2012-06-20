using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IVirtualSchedulePeriodProvider
	{
		IVirtualSchedulePeriod VirtualSchedulePeriodForDate(DateOnly date);
		DateOnlyPeriod GetCurrentOrNextVirtualPeriodForDate(DateOnly date);
		bool HasSchedulePeriod();
		bool MissingPersonPeriod();
		DateOnly CalculateStudentAvailabilityDefaultDate();
		DateOnly CalculatePreferenceDefaultDate();
	}
}