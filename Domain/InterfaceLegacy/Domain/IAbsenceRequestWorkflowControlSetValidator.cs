namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestWorkflowControlSetValidator
	{
		IValidatedRequest Validate(IPersonRequest personRequest);
	}
}