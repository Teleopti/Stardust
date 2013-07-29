namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{
    public interface IState:IStateReader
    {
        void SetSessionData(ISessionData sessionData);
    }
}