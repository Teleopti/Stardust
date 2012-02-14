
namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{
    public abstract class State : IState
    {
        public abstract void SetSessionData(ISessionData sessionData);
        public abstract void ClearSession();
        public abstract ISessionData SessionScopeData { get; }

        public bool IsLoggedIn
        {
            get { return (SessionScopeData != null); }
        }
    }
}