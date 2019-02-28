using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for SiteRepository
    /// </summary>
    public class SiteRepository : Repository<ISite>, ISiteRepository
    {
		public static SiteRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SiteRepository(currentUnitOfWork, null, null);
		}

		public static SiteRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new SiteRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public SiteRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
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