namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestPersonAccountValidator
	{
		IValidatedRequest Validate(IPersonRequest personRequest, IScheduleRange scheduleRange);
	}
}