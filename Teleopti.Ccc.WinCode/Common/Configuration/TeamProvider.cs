using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class TeamProvider : ITeamProvider
    {
        private readonly ITeamRepository _teamRepository;
        private IList<ITeam> _teamCollection;
        private static readonly object LockObject = new object();

        public TeamProvider(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public IEnumerable<ITeam> GetTeams()
        {
            EnsureInitialized();
            return _teamCollection;
        }

        private void EnsureInitialized()
        {
            if (_teamCollection == null)
            {
                _teamCollection = _teamRepository.LoadAll();
            }
        }

        public void HandleMessageBrokerEvent(Guid domainObjectId, DomainUpdateType domainUpdateType)
        {
            lock (LockObject)
            {
                EnsureInitialized();
                if (domainUpdateType == DomainUpdateType.Delete ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var currentSite = _teamCollection.FirstOrDefault(s => s.Id == domainObjectId);
                    _teamCollection.Remove(currentSite);
                }
                if (domainUpdateType == DomainUpdateType.Insert ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var newSite = _teamRepository.Get(domainObjectId);
                    _teamCollection.Add(newSite);
                }
            }
        }
    }
}