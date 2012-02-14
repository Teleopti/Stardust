using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class PendingAbsenceRequest : ProcessAbsenceRequest
    {
        public override string DisplayText
        {
            get { return UserTexts.Resources.No; }
        }

        public override IProcessAbsenceRequest CreateInstance()
        {
            return new PendingAbsenceRequest();
        }

        public override void Process(IPerson processingPerson,IAbsenceRequest absenceRequest, IPersonRequestCheckAuthorization authorization, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
        {
            if (!CheckValidatorList(processingPerson, absenceRequest, authorization, absenceRequestValidatorList)) return;

            UndoAll();

            IPersonRequest personRequest = (IPersonRequest)absenceRequest.Parent;
            personRequest.Pending();
        }

        public override bool Equals(object obj)
        {
            PendingAbsenceRequest process = obj as PendingAbsenceRequest;
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