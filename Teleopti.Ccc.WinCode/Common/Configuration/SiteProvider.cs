using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class SiteProvider : ISiteProvider
    {
        private readonly ISiteRepository _siteRepository;
        private readonly Lazy<IList<ISite>> _siteCollection;
        private static readonly ISite _allSitesItem = new Site(UserTexts.Resources.AllSelection);
        private static readonly object LockObject = new object();

        public SiteProvider(ISiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
	        _siteCollection = new Lazy<IList<ISite>>(() =>
	        {
		        var sites = _siteRepository.LoadAll().OrderBy(s => s.Description.Name).ToList();
		        sites.Insert(0, _allSitesItem);
		        return sites;
	        });
        }

        public IList<ISite> GetSitesAllSitesItemIncluded()
        {
		    return _siteCollection.Value;
        }

        public ISite AllSitesItem
        {
            get { return _allSitesItem; }
        }

        public void HandleMessageBrokerEvent(Guid domainObjectId, DomainUpdateType domainUpdateType)
        {
            lock (LockObject)
            {
                if (domainUpdateType == DomainUpdateType.Delete ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var currentSite = _siteCollection.Value.FirstOrDefault(s => s.Id == domainObjectId);
                    _siteCollection.Value.Remove(currentSite);
                }
                if (domainUpdateType == DomainUpdateType.Insert ||
                    domainUpdateType == DomainUpdateType.Update)
                {
                    var newSite = _siteRepository.Get(domainObjectId);
                    _siteCollection.Value.Add(newSite);
                }
            }
        }
    }
}