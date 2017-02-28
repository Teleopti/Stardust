namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestSynchronousValidator
	{
		IValidatedRequest Validate(IPersonRequest personRequest);
	}
}
