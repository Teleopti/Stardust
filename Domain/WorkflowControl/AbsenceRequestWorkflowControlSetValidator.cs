using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequestWorkflowControlSetValidator : IAbsenceRequestWorkflowControlSetValidator
	{
		public IValidatedRequest Validate(IPersonRequest personRequest)
		{
			var person = personRequest.Person;
			var workflowControlSet = person.WorkflowControlSet;
			if (workflowControlSet == null)
				return new ValidatedRequest { IsValid = false, ValidationErrors = nameof(Resources.RequestDenyReasonNoWorkflow) };

			var absenceRequestOpenPeriod =
				workflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest) personRequest.Request);

			if (absenceRequestOpenPeriod.AbsenceRequestProcess is DenyAbsenceRequest absenceRequestProcess)
				return new ValidatedRequest {IsValid = false, ValidationErrors = absenceRequestProcess.DenyReason};

			return ValidatedRequest.Valid;
		}
	}
}