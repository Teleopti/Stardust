using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public abstract class ProcessAbsenceRequest : IProcessAbsenceRequest
    {
        public abstract string DisplayText { get; }
        public abstract IProcessAbsenceRequest CreateInstance();

        protected bool CheckValidatorList(IPerson processingPerson, IAbsenceRequest absenceRequest, RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
        {
            foreach (var absenceRequestValidator in absenceRequestValidatorList)
            {
                IValidatedRequest validatedRequest = absenceRequestValidator.Validate(absenceRequest, requiredForHandlingAbsenceRequest);

                if(! validatedRequest.IsValid)
                {
                    var denyAbsenceRequest = new DenyAbsenceRequest
                                                                {
                                                                    DenyReason = validatedRequest.ValidationErrors,
                                                                };
                    denyAbsenceRequest.Process(processingPerson, absenceRequest,requiredForProcessingAbsenceRequest, requiredForHandlingAbsenceRequest,null);
                    return false;
                }
            }
            return true;
        }

        protected void UndoAll(RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest)
        {
            if (requiredForProcessingAbsenceRequest.UndoRedoContainer!=null)
            {
                requiredForProcessingAbsenceRequest.UndoRedoContainer.UndoAll();
            }
        }

        public abstract void Process(IPerson processingPerson, IAbsenceRequest absenceRequest, RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList);
    }
}