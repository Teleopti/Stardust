using System.Collections.Generic;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
    public class GrantAbsenceRequest : ProcessAbsenceRequest
    {
        private readonly static ILog Logger = LogManager.GetLogger(typeof (GrantAbsenceRequest));

        public override string DisplayText
        {
            get { return UserTexts.Resources.Yes; }
        }

        public override IProcessAbsenceRequest CreateInstance()
        {
            return new GrantAbsenceRequest();
        }

        public override void Process(IPerson processingPerson, IAbsenceRequest absenceRequest, RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest, RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest, IEnumerable<IAbsenceRequestValidator> absenceRequestValidatorList)
        {
            InParameter.NotNull("RequestApprovalService", requiredForProcessingAbsenceRequest.RequestApprovalService);
            if (!CheckValidatorList(processingPerson, absenceRequest, requiredForProcessingAbsenceRequest, requiredForHandlingAbsenceRequest, absenceRequestValidatorList)) return;
            
            UndoAll(requiredForProcessingAbsenceRequest);
	        requiredForProcessingAbsenceRequest.AfterUndoCallback();

            IPersonRequest personRequest = (IPersonRequest)absenceRequest.Parent;
            personRequest.Pending();
            var result = personRequest.Approve(requiredForProcessingAbsenceRequest.RequestApprovalService,requiredForProcessingAbsenceRequest.Authorization);
            foreach (IBusinessRuleResponse ruleResponse in result)
            {
               if(Logger.IsWarnEnabled)
                    Logger.WarnFormat("At least one validation rule failed, the schedule cannot be changed. The error was: {0}",ruleResponse.Message);
            }
        }

        public override bool Equals(object obj)
        {
            GrantAbsenceRequest process = obj as GrantAbsenceRequest;
            return process!=null;
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