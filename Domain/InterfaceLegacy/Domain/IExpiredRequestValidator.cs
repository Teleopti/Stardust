namespace Teleopti.Interfaces.Domain
{
	public interface IExpiredRequestValidator
	{
		IValidatedRequest ValidateExpiredRequest(IAbsenceRequest absenceRequest, IScheduleRange scheduleRange);
	}
}