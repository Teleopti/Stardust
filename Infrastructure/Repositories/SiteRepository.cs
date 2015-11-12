using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
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
                     .SetResultTransformer(Transformers.DistinctRootEntity)
                     .List<ISite>();
            return retList;
        }

	    public IEnumerable<ISite> FindSitesStartWith(string searchString)
	    {
		    //TODO fix later
		    return Enumerable.Empty<ISite>();
	    }
    }
}