using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequestWorkflowControlSetValidator : IAbsenceRequestWorkflowControlSetValidator
	{
		public IValidatedRequest Validate(IPersonRequest personRequest)
		{
			var person = personRequest.Person;
			var workflowControlSet = person.WorkflowControlSet;
			var absenceRequestOpenPeriod =
				workflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest) personRequest.Request);
			var absenceRequestProcess = absenceRequestOpenPeriod.AbsenceRequestProcess as DenyAbsenceRequest;
			if (absenceRequestProcess != null)
				return new ValidatedRequest {IsValid = false, ValidationErrors = absenceRequestProcess.DenyReason};
			return new ValidatedRequest {IsValid = true};
		}
	}
}