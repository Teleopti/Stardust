using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public interface IStudentAvailabilityFeedbackProvider
	{
		WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date, IScheduleDay scheduleDay);
		WorkTimeMinMaxCalculationResult WorkTimeMinMaxForDate(DateOnly date);
	}
}