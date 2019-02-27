using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for applicationrole root
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-09
    /// </remarks>
    public class ApplicationRoleRepository : Repository<IApplicationRole>, IApplicationRoleRepository
    {
		public static ApplicationRoleRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new ApplicationRoleRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public static ApplicationRoleRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ApplicationRoleRepository(currentUnitOfWork, null, null);
		}
		
		public ApplicationRoleRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
	    {
		    
	    }

        /// <summary>
        /// Reads all application roles ordered by their name.
        /// </summary>
        /// <returns>The ApplicationRole list.</returns>
        /// <remarks>
        /// Created By: kosalanp
        /// Created Date: 08-04-2008
        /// </remarks>
        public virtual IList<IApplicationRole> LoadAllApplicationRolesSortedByName()
        {
	        var appRoles = Session.CreateCriteria(typeof (ApplicationRole))
		        .Fetch("ApplicationFunctionCollection")
		        .AddOrder(Order.Asc("Name"))
		        .SetResultTransformer(new DistinctRootEntityResultTransformer())
		        .List<IApplicationRole>()
		        .Where(x => x.AvailableData != null)
		        .ToList();
           
            return appRoles;
        }
		
		public virtual IList<IApplicationRole> LoadAllRolesByDescription(string role)
	    {
			 var appRoles = Session.CreateCriteria(typeof(ApplicationRole))
				  .Fetch("ApplicationFunctionCollection")
				  .AddOrder(Order.Asc("DescriptionText"))
				  .SetResultTransformer(new DistinctRootEntityResultTransformer())
				  .List<IApplicationRole>()
				  .Where(x => x.DescriptionText.ToLower().Contains(role.ToLower()))
				  .ToList();

			 return appRoles;
	    }

	    public bool ExistsRoleWithDescription(string description)
	    {
		    var criteria = Session.CreateCriteria(typeof(ApplicationRole));
		    criteria.Add(Restrictions.Eq(Projections.Property<ApplicationRole>(a => a.DescriptionText), description));
		    criteria.SetProjection(Projections.RowCount());
		    return criteria.UniqueResult<int>() > 0;
	    }
    }
}
