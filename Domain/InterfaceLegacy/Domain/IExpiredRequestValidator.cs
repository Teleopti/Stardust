namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IExpiredRequestValidator
	{
		IValidatedRequest ValidateExpiredRequest(IAbsenceRequest absenceRequest, IScheduleRange scheduleRange);
	}
}