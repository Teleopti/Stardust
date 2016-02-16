using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class TeamProvider : ITeamProvider
    {
        private readonly ITeamRepository _teamRepository;
        private readonly Lazy<IList<ITeam>> _teamCollection;
        private static readonly object LockObject = new object();

        public TeamProvider(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
			_teamCollection = new Lazy<IList<ITeam>>(()=>_teamRepository.LoadAll());
        }

        public IEnumerable<ITeam> GetTeams()
        {
            return _teamCollection.Value;
        }

        public void HandleMessageBrokerEvent(Guid domainObjectId, DomainUpdateType domainUpdateType)
        {
            lock (LockObject)
            {
                if (domainUpdateType == DomainUpdateType.Delete ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var currentSite = _teamCollection.Value.FirstOrDefault(s => s.Id == domainObjectId);
                    _teamCollection.Value.Remove(currentSite);
                }
                if (domainUpdateType == DomainUpdateType.Insert ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var newSite = _teamRepository.Get(domainObjectId);
                    _teamCollection.Value.Add(newSite);
                }
            }
        }
    }
}