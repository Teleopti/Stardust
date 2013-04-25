using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Client;

namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{
    public interface ISessionData : ISessionStateProvider
    {
        IDictionary<string, string> AppSettings { get; }
        void AssignAppSettings(IDictionary<string, string> appSettings);
    	void SetPassword(string password);
    }
}