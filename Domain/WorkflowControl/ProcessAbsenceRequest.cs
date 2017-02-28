using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public abstract class ProcessAbsenceRequest : IProcessAbsenceRequest
	{
		public abstract string DisplayText { get; }
		public abstract IProcessAbsenceRequest CreateInstance();

		protected bool CheckValidatorList(IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
		{
			foreach (var absenceRequestValidator in absenceRequestValidatorList)
			{
				IValidatedRequest validatedRequest = absenceRequestValidator.Validate(absenceRequest,
					requiredForHandlingAbsenceRequest);

				if (!validatedRequest.IsValid)
				{
					var denyAbsenceRequest = new DenyAbsenceRequest
					{
						DenyReason = validatedRequest.ValidationErrors,
						DenyOption = validatedRequest.DenyOption
					};
					denyAbsenceRequest.Process(absenceRequest, requiredForProcessingAbsenceRequest,
						requiredForHandlingAbsenceRequest, null);
					return false;
				}
			}
			return true;
		}

		protected void UndoAll(RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, IAbsenceRequest request)
		{
			requiredForProcessingAbsenceRequest.UndoRedoContainer?.UndoAll();
		}

		public abstract void Process(IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList);
	}
}