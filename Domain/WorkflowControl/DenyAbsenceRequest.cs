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
            return new DenyAbsenceRequest {DenyReason = DenyReason};
        }

        public override void Process(IPerson processingPerson, IAbsenceRequest absenceRequest, RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
        {
            UndoAll(requiredForProcessingAbsenceRequest);
            var personRequest = (IPersonRequest)absenceRequest.Parent;
            personRequest.Deny(processingPerson, DenyReason, requiredForProcessingAbsenceRequest.Authorization, true);
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
                int result = (GetType().GetHashCode());
                return result;
            }
        }
    }
}