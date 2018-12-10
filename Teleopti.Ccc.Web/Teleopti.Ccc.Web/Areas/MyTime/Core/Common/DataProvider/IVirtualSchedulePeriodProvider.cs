using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IVirtualSchedulePeriodProvider
	{
		IVirtualSchedulePeriod VirtualSchedulePeriodForDate(DateOnly date);
		DateOnlyPeriod GetCurrentOrNextVirtualPeriodForDate(DateOnly date);
		bool MissingSchedulePeriod();
		bool MissingPersonPeriod(DateOnly? date);
		DateOnly CalculateStudentAvailabilityDefaultDate();
		DateOnly CalculatePreferenceDefaultDate();
	}
}