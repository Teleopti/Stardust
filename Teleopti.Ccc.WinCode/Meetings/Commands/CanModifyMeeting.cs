using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Meetings.Overview;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
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
                if (_model.CurrentScenario.Restricted && !TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario))
                {
                    return false;
                }
                if (!TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings))
                {
                    return false;
                }
                return true;
            }
        }
    }
}