using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public interface IScorecardProvider
    {
        IEnumerable<IScorecard> GetScorecards();
        void HandleMessageBrokerEvent(Guid domainObjectId, DomainUpdateType domainUpdateType);
    }
}