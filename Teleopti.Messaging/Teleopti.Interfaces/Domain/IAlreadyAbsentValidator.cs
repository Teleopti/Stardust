namespace Teleopti.Interfaces.Domain
{
	public interface IAlreadyAbsentValidator
	{
		bool Validate(IAbsenceRequest absenceRequest, IScheduleRange scheduleRange);
	}
}