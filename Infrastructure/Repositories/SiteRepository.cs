using System.Collections.Generic;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public SiteRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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
    }
}