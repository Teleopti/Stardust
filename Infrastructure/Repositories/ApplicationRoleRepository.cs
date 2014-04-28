using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationRoleRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-09
        /// </remarks>
        public ApplicationRoleRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

		 public ApplicationRoleRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

		 public ApplicationRoleRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
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
                .SetFetchMode("ApplicationFunctionCollection", FetchMode.Join)
                .AddOrder(Order.Asc("Name"))
                .SetResultTransformer(new DistinctRootEntityResultTransformer())
                .List<IApplicationRole>();
           
            return appRoles;
        }

        public override bool ValidateUserLoggedOn
        {
            get
            {
                return false;
            }
        }
    }
}
