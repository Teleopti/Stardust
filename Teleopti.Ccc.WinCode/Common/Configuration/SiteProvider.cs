using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class SiteProvider : ISiteProvider
    {
        private readonly ISiteRepository _siteRepository;
        private readonly bool _includeAllSitesItem;
        private IList<ISite> _siteCollection;
        private static readonly ISite _allSitesItem = new Site(UserTexts.Resources.AllSelection);
        private static readonly object LockObject = new object();

        public SiteProvider(ISiteRepository siteRepository, bool includeAllSitesItem)
        {
            _siteRepository = siteRepository;
            _includeAllSitesItem = includeAllSitesItem;
        }

        public IList<ISite> GetSites()
        {
            EnsureInitialized();
            return _siteCollection;
        }

        private void EnsureInitialized()
        {
            if (_siteCollection == null)
            {
                var sites = _siteRepository.LoadAll().OrderBy(s => s.Description.Name).ToList();
                if (_includeAllSitesItem)
                    sites.Insert(0, _allSitesItem);
                _siteCollection = sites;
            }
        }

        public ISite AllSitesItem
        {
            get { return _allSitesItem; }
        }

        public void HandleMessageBrokerEvent(Guid domainObjectId, DomainUpdateType domainUpdateType)
        {
            lock (LockObject)
            {
                EnsureInitialized();
                if (domainUpdateType == DomainUpdateType.Delete ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var currentSite = _siteCollection.FirstOrDefault(s => s.Id == domainObjectId);
                    _siteCollection.Remove(currentSite);
                }
                if (domainUpdateType == DomainUpdateType.Insert ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var newSite = _siteRepository.Get(domainObjectId);
                    _siteCollection.Add(newSite);
                    _siteCollection = _siteCollection.OrderBy(s => s.Description.Name).ToList();
                }
            }
        }
    }
}