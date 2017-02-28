namespace Teleopti.Interfaces.Domain
{
	public interface IAbsenceRequestWorkflowControlSetValidator
	{
		IValidatedRequest Validate(IPersonRequest personRequest);
	}
}