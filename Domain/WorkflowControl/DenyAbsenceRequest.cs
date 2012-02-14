using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class DenyAbsenceRequest : ProcessAbsenceRequest
    {
        public DenyAbsenceRequest()
        {
            DenyReason = "RequestDenyReasonAutodeny";
        }

        public override string DisplayText
        {
            get { return UserTexts.Resources.Deny; }
        }

        public string DenyReason { get; set; }

        public override IProcessAbsenceRequest CreateInstance()
        {
            return new DenyAbsenceRequest {UndoRedoContainer = UndoRedoContainer, DenyReason = DenyReason};
        }

        public override void Process(IPerson processingPerson, IAbsenceRequest absenceRequest, IPersonRequestCheckAuthorization authorization, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
        {
            UndoAll();
            var personRequest = (IPersonRequest)absenceRequest.Parent;
            personRequest.Deny(processingPerson, DenyReason, authorization);
        }

        public override bool Equals(object obj)
        {
            DenyAbsenceRequest process = obj as DenyAbsenceRequest;
            return process != null;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (RequestApprovalService != null ? RequestApprovalService.GetHashCode() : 0);
                result = (result * 397) ^ (GetType().GetHashCode());
                return result;
            }
        }
    }
}