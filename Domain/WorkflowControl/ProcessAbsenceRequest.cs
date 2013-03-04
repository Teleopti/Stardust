using System.Collections.Generic;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public abstract class ProcessAbsenceRequest : IProcessAbsenceRequest
    {
        public IRequestApprovalService RequestApprovalService { get; set; }
        public IUndoRedoContainer UndoRedoContainer { get; set; }
        public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }

        public abstract string DisplayText { get; }
        public abstract IProcessAbsenceRequest CreateInstance();

        protected bool CheckValidatorList(IPerson processingPerson, IAbsenceRequest absenceRequest, IPersonRequestCheckAuthorization authorization, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
        {
            foreach (var absenceRequestValidator in absenceRequestValidatorList)
            {
                IValidatedRequest validatedRequest = absenceRequestValidator.Validate(absenceRequest);

                //if (!absenceRequestValidator.Validate(absenceRequest))
                if(! validatedRequest.IsValid)
                {
                    //DenyAbsenceRequest denyAbsenceRequest = new DenyAbsenceRequest
                    //                                            {
                    //                                                DenyReason = string.Format(absenceRequest.Person.PermissionInformation.Culture(),
                    //                                                UserTexts.Resources.ResourceManager.GetString(absenceRequestValidator.InvalidReason)),
                    //                                                UndoRedoContainer = UndoRedoContainer
                    //                                            };

                    var denyAbsenceRequest = new DenyAbsenceRequest
                                                                {
                                                                    DenyReason = validatedRequest.ValidationErrors,
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