using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IVirtualSchedulePeriodProvider
	{
		DateOnlyPeriod GetCurrentOrNextVirtualPeriodForDate(DateOnly date);
		bool HasSchedulePeriod();
		DateOnly CalculateStudentAvailabilityDefaultDate();
		DateOnly CalculatePreferenceDefaultDate();
	}
}