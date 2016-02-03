using System.Collections.Generic;
using System.Globalization;
using NHibernate.Criterion;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public ScenarioRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
	    }
		
        public IList<IScenario> FindAllSorted()
        {
            IList<IScenario> scenarios = Session.CreateCriteria(typeof (Scenario))
                .List<IScenario>();

            return scenarios.OrderByDescending(sc => sc.DefaultScenario).ThenBy(sc => sc.Description.Name).ToList();
        }

        public IList<IScenario> FindEnabledForReportingSorted()
        {
            IList<IScenario> scenarios = Session.CreateCriteria(typeof(Scenario))
                .Add(Property.ForName("EnableReporting").Eq(true))
                .List<IScenario>();

			return scenarios.OrderByDescending(sc => sc.DefaultScenario).ThenBy(sc => sc.Description.Name).ToList();
		}
		
        public IScenario LoadDefaultScenario()
        {
            var defaultScenario =  Session.CreateCriteria(typeof(Scenario))
                 .Add(Property.ForName("DefaultScenario").Eq(true))
                 .UniqueResult<IScenario>();
			if (defaultScenario == null)
			{
				var bu = ServiceLocatorForEntity.CurrentBusinessUnit.Current();
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
