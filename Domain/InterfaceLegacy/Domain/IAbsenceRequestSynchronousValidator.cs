namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestSynchronousValidator
	{
		IValidatedRequest Validate(IPersonRequest personRequest);

		IValidatedRequest Validate(IPersonRequest personRequest, IScheduleRange scheduleRange,
			IPersonAbsenceAccount personAbsenceAccount);
	}
}
