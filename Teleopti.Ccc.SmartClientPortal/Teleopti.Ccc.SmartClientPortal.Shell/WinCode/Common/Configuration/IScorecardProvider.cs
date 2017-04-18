using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
    public interface IScorecardProvider
    {
        IEnumerable<IScorecard> GetScorecards();
        void HandleMessageBrokerEvent(Guid domainObjectId, DomainUpdateType domainUpdateType);
    }
}