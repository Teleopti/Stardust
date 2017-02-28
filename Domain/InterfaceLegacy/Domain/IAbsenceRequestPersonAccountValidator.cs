namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestPersonAccountValidator
	{
		IValidatedRequest Validate(IPersonRequest personRequest, IScheduleRange scheduleRange);
	}
}