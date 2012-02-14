using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public interface ISiteProvider
    {
        IList<ISite> GetSites();
        ISite AllSitesItem { get; }
        void HandleMessageBrokerEvent(Guid domainObjectId, DomainUpdateType domainUpdateType);
    }
}