using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for SiteRepository
    /// </summary>
    public class SiteRepository : Repository<ISite>, ISiteRepository
    {
#pragma warning disable 618
        public SiteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public SiteRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        public ICollection<ISite> FindSiteByDescriptionName(string name)
        {
            ICollection<ISite> retList = Session.CreateCriteria<Site>()
                      .Add(Restrictions.Eq("Description.Name", name))
                     .List<ISite>();
            return retList;
        }

	    public IEnumerable<ISite> FindSitesContain(string searchString, int maxHits)
	    {
		    if (maxHits < 1)
			    return Enumerable.Empty<ISite>();

			return Session.CreateCriteria<Site>()
				.Add(Restrictions.Like("Description.Name", searchString, MatchMode.Anywhere))
				.SetMaxResults(maxHits)
				.List<ISite>();
		}

	    public IEnumerable<ISite> LoadAllOrderByName()
	    {
			return Session.CreateCriteria<Site>()
				.AddOrder(Order.Asc("Description.Name"))
				.List<ISite>();
			
	    }

	    public IEnumerable<ISite> LoadAllWithFetchingOpenHours()
	    {
			return Session.CreateCriteria<Site>()
				.Fetch("OpenHourCollection")
				.List<ISite>();
		}
	}
}