using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    public class PersonRequestAuthorization
    {
        private readonly IAuthorization _authorization;

        public PersonRequestAuthorization(IAuthorization authorization)
        {
            _authorization = authorization;
        }


        public bool IsPermittedRequestView()
        {
            return (_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler));
        }


        public bool IsPermittedRequestApprove(string requestType)
        {
            if (requestType == Resources.RequestTypeText)
                return IsPermittedTextRequestApprove();
            return IsPermittedAbsenceOrShiftTradeRequestApprove();
        }

        private bool IsPermittedAbsenceOrShiftTradeRequestApprove()
        {
            if (!_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove))
                return false;
            if (!_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifySchedule))
                return false;
            return true;
        }

        private bool IsPermittedTextRequestApprove()
        {
            return (_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestSchedulerApprove));
        }



    }
}