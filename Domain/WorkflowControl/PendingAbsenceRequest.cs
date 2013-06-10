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

        public override void Process(IPerson processingPerson, IAbsenceRequest absenceRequest, RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
        {
            if (!CheckValidatorList(processingPerson, absenceRequest, requiredForProcessingAbsenceRequest, requiredForHandlingAbsenceRequest, absenceRequestValidatorList)) return;

            UndoAll(requiredForProcessingAbsenceRequest);

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
                int result = (GetType().GetHashCode());
                return result;
            }
        }
    }
}