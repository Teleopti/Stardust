using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public abstract class ProcessAbsenceRequest : IProcessAbsenceRequest
    {
        public IRequestApprovalService RequestApprovalService { get; set; }
        public IUndoRedoContainer UndoRedoContainer { get; set; }

        public abstract string DisplayText { get; }
        public abstract IProcessAbsenceRequest CreateInstance();

        protected bool CheckValidatorList(IPerson processingPerson, IAbsenceRequest absenceRequest, IPersonRequestCheckAuthorization authorization, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
        {
            foreach (var absenceRequestValidator in absenceRequestValidatorList)
            {
                if (!absenceRequestValidator.Validate(absenceRequest))
                {
                    DenyAbsenceRequest denyAbsenceRequest = new DenyAbsenceRequest
                                                                {
                                                                    DenyReason = absenceRequestValidator.InvalidReason,
                                                                    UndoRedoContainer = UndoRedoContainer
                                                                };
                    denyAbsenceRequest.Process(processingPerson, absenceRequest,authorization,null);
                    return false;
                }
            }
            return true;
        }

        protected void UndoAll()
        {
            if (UndoRedoContainer!=null)
            {
                UndoRedoContainer.UndoAll();
            }
        }

        public abstract void Process(IPerson processingPerson, IAbsenceRequest absenceRequest, IPersonRequestCheckAuthorization authorization, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList);
    }
}