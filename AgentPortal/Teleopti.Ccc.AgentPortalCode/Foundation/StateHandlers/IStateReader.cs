
namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{
    public interface IStateReader
    {
        bool IsLoggedIn { get; }
        ISessionData SessionScopeData { get; }
    }
}