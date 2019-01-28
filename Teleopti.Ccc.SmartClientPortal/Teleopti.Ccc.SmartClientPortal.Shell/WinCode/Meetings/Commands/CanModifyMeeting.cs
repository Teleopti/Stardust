using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands
{
    public interface ICanModifyMeeting
    {
        bool CanExecute { get; }
    }

    public class CanModifyMeeting : ICanModifyMeeting
    {
        private readonly IMeetingOverviewViewModel _model;

        public CanModifyMeeting(IMeetingOverviewViewModel model)
        {
            _model = model;
        }

        public bool CanExecute
        {
            get
            {
                if (_model.CurrentScenario.Restricted && !PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario))
                {
                    return false;
                }
                if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings))
                {
                    return false;
                }
                return true;
            }
        }
    }
}