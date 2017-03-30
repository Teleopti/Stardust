using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferenceFeedbackProvider
	{
		WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay);
		WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date);
		PreferenceNightRestCheckResult CheckNightRestViolation(DateOnly date);
		IDictionary<DateOnly, PreferenceNightRestCheckResult> CheckNightRestViolation(DateOnlyPeriod period);
	}
}