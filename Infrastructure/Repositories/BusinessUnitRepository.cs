using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository methods for business units.
    /// </summary>
    public class BusinessUnitRepository : Repository<IBusinessUnit>, IBusinessUnitRepository
    {
        /// <summary>
        /// Initilaze a new instance of the <see cref="BusinessUnitRepository"/> class 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public BusinessUnitRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

			public BusinessUnitRepository(ICurrentUnitOfWork currentUnitOfWork)
				: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Finds all business units sorted by name.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2008-05-22
        /// </remarks>
        public IList<IBusinessUnit> LoadAllBusinessUnitSortedByName()
        {
            IList<IBusinessUnit> functions = Session.CreateCriteria(typeof(BusinessUnit))
                .AddOrder(Order.Asc("Description.Name"))
                .List<IBusinessUnit>();

            return functions;
        }

        /// <summary>
        /// Loads the hierarchy information.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <returns></returns>
        public IBusinessUnit LoadHierarchyInformation(IBusinessUnit businessUnit)
        {
            var sites = DetachedCriteria.For<BusinessUnit>()
                .Add(Restrictions.Eq("Id", businessUnit.Id))
                .SetFetchMode("SiteCollection", FetchMode.Join);

            var sitesForTeams = DetachedCriteria.For<BusinessUnit>()
                .Add(Restrictions.Eq("Id", businessUnit.Id))
                .CreateAlias("SiteCollection", "site")
                .SetProjection(Projections.Property("site.Id"));
            var teams = Session.CreateCriteria<Site>()
                .Add(Subqueries.PropertyIn("Id", sitesForTeams))
                .SetFetchMode("TeamCollection", FetchMode.Join);

            Session.DisableFilter("businessUnitFilter");

            var result = Session.CreateMultiCriteria()
                .Add(sites)
                .Add(teams)
                .SetResultTransformer(Transformers.DistinctRootEntity).List();

            var foundBueinessUnit = CollectionHelper.ToDistinctGenericCollection<BusinessUnit>(result[0]).First();
            return foundBueinessUnit;
        }

        /// <summary>
        /// Loads all time zones.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-08-11
        /// </remarks>
        public IEnumerable<TimeZoneInfo> LoadAllTimeZones()
        {
            IList<string> skillTimeZoneId = Session.CreateCriteria(typeof (Skill))
                .Add(Restrictions.IsNotNull("TimeZone"))
                .SetProjection(Projections.GroupProperty("TimeZone"))
                .List<string>();

            IList<string> personTimeZoneId = Session.CreateCriteria(typeof(Person))
                .Add(Restrictions.IsNotNull("PermissionInformation.defaultTimeZone"))
                .SetProjection(Projections.GroupProperty("PermissionInformation.defaultTimeZone"))
                .List<string>();

            return skillTimeZoneId
                .Concat(personTimeZoneId)
                .Distinct().Select(TimeZoneInfo.FindSystemTimeZoneById).ToList();
        }

		public IList<Guid> LoadAllPersonsWithExternalLogOn(Guid businessUnitId, DateOnly now)
		{
			var result = Session.CreateSQLQuery(
				"exec dbo.LoadPersonsWithExternalLogOn @businessUnitId=:businessUnitId, @now=:now")
			                    .SetGuid("businessUnitId", businessUnitId)
			                    .SetDateTime("now", now.Date)
			                    .List<Guid>();
			return result;
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