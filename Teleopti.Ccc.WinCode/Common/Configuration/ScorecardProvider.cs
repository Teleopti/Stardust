using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class ScorecardProvider : IScorecardProvider
    {
        private readonly IRepository<IScorecard> _scorecardRepository;
        private readonly bool _includeNullScorecardItem;
        private IList<IScorecard> _scorecardCollection;
        public static readonly IScorecard NullScorecard = new Scorecard { Name = string.Empty };
        private static readonly object LockObject = new object();

        public ScorecardProvider(IRepository<IScorecard> scorecardRepository, bool includeNullScorecardItem)
        {
            _scorecardRepository = scorecardRepository;
            _includeNullScorecardItem = includeNullScorecardItem;
        }

        public IEnumerable<IScorecard> GetScorecards()
        {
            EnsureInitialized();
            return _scorecardCollection;
        }

        private void EnsureInitialized()
        {
            if (_scorecardCollection==null)
            {
                var result = _scorecardRepository.LoadAll();
                if (_includeNullScorecardItem)
                    result.Add(NullScorecard);
                _scorecardCollection = result;
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
                    var currentSite = _scorecardCollection.FirstOrDefault(s => s.Id == domainObjectId);
                    _scorecardCollection.Remove(currentSite);
                }
                if (domainUpdateType == DomainUpdateType.Insert ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var newSite = _scorecardRepository.Get(domainObjectId);
                    _scorecardCollection.Add(newSite);
                }
            }
        }
    }
}