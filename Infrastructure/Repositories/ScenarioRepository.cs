using System.Collections.Generic;
using System.Globalization;
using NHibernate.Criterion;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for scenarios
    /// </summary>
    public class ScenarioRepository : Repository<IScenario>, IScenarioRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public ScenarioRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public ScenarioRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

				public ScenarioRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }

        /// <summary>
        /// Finds all scenarios.
        /// Default first, then sorted by name
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-07
        /// </remarks>
        public IList<IScenario> FindAllSorted()
        {
            IList<IScenario> scenarios = Session.CreateCriteria(typeof (Scenario))
                //.AddOrder(Order.Desc("DefaultScenario"))
                //.AddOrder(Order.Asc("Description.Name"))
                .List<IScenario>();

            return scenarios.OrderBy(sc => sc.Description.Name).OrderByDescending(sc => sc.DefaultScenario).ToList();
        }

        public IList<IScenario> FindEnabledForReportingSorted()
        {
            IList<IScenario> scenarios = Session.CreateCriteria(typeof(Scenario))
                .Add(Property.ForName("EnableReporting").Eq(true))
                .List<IScenario>();

            return scenarios.OrderBy(sc => sc.Description.Name).OrderByDescending(sc => sc.DefaultScenario).ToList();
        }

        /// <summary>
        /// Loads the defaut scenario.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumeda Herath
        /// Created date: 2008-03-05
        /// </remarks>
        public IScenario LoadDefaultScenario()
        {
            var defaultScenario =  Session.CreateCriteria(typeof(Scenario))
                 .Add(Property.ForName("DefaultScenario").Eq(true))
                 .UniqueResult<IScenario>();
			if (defaultScenario == null)
			{
				var bu = CurrentBusinessUnit.Instance.Current();
				throw new DataSourceException(string.Format(CultureInfo.CurrentCulture, "Business unit '{0}' has no default scenario", bu.Name));
			}
        	return defaultScenario;
        }

        public IScenario LoadDefaultScenario(IBusinessUnit businessUnit)
        {
            using(UnitOfWork.DisableFilter(QueryFilter.BusinessUnit))
            {
                return Session.CreateCriteria(typeof(Scenario))
                 .Add(Property.ForName("DefaultScenario").Eq(true))
                 .Add(Property.ForName("BusinessUnit").Eq(businessUnit))
                 .UniqueResult<IScenario>();
            }
        }
    }
}
