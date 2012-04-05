using System;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public class BasicState : State
    {
		[ThreadStatic]
		private static ISessionData _sessionScopeData;

        public override void SetSessionData(ISessionData sessionData)
        {
            _sessionScopeData = sessionData;
        }

        public override ISessionData SessionScopeData { get { return _sessionScopeData; } }

        public override void ClearSession()
        {
            _sessionScopeData = null;
        }
    }
}