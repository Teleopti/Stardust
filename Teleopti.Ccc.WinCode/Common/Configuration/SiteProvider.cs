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
        private IList<ISite> _siteCollection;
        private static readonly ISite _allSitesItem = new Site(UserTexts.Resources.AllSelection);
        private static readonly object LockObject = new object();

        public SiteProvider(ISiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        public IList<ISite> GetSitesAllSitesItemIncluded()
        {
			EnsureInitialized(isAllSitesItemIncluded: true);
            return _siteCollection;
        }

		public IList<ISite> GetSitesAllSitesItemNotIncluded()
        {
			EnsureInitialized(isAllSitesItemIncluded: false);
            return _siteCollection;
        }

        private void EnsureInitialized(bool isAllSitesItemIncluded)
        {
            if (_siteCollection == null)
            {
                var sites = _siteRepository.LoadAll().OrderBy(s => s.Description.Name).ToList();
                if (isAllSitesItemIncluded)
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
				EnsureInitialized(isAllSitesItemIncluded: false);
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