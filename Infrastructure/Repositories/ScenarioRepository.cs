using System;
using System.Collections.Generic;
using System.Globalization;
using NHibernate.Criterion;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScenarioRepository : Repository<IScenario>, IScenarioRepository
	{
		public static ScenarioRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ScenarioRepository(currentUnitOfWork, null, null);
		}

		public static ScenarioRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new ScenarioRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public ScenarioRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		public IList<IScenario> FindAllSorted()
		{
			var scenarios = Session.CreateCriteria(typeof(Scenario))
				.List<IScenario>();

			return scenarios.OrderByDescending(sc => sc.DefaultScenario).ThenBy(sc => sc.Description.Name).ToList();
		}

		public IList<IScenario> FindEnabledForReportingSorted()
		{
			var scenarios = Session.CreateCriteria(typeof(Scenario))
				.Add(Property.ForName("EnableReporting").Eq(true))
				.List<IScenario>();

			return scenarios.OrderByDescending(sc => sc.DefaultScenario).ThenBy(sc => sc.Description.Name).ToList();
		}

		public IScenario LoadDefaultScenario()
		{
			var defaultScenario = Session.CreateCriteria(typeof(Scenario))
				.Add(Property.ForName("DefaultScenario").Eq(true))
				.UniqueResult<IScenario>();
			if (defaultScenario == null)
			{
				var businessUnitId = ServiceLocator_DONTUSE.CurrentBusinessUnit.CurrentId();
				throw new NoDefaultScenarioException(string.Format(CultureInfo.CurrentCulture,
					"Business unit '{0}' has no default scenario", businessUnitId));
			}
			return defaultScenario;
		}

		public IScenario LoadDefaultScenario(IBusinessUnit businessUnit)
		{
			using (UnitOfWork.DisableFilter(QueryFilter.BusinessUnit))
			{
				return Session.CreateCriteria(typeof(Scenario))
					.Add(Property.ForName("DefaultScenario").Eq(true))
					.Add(Property.ForName("BusinessUnit").Eq(businessUnit))
					.UniqueResult<IScenario>();
			}
		}

	}
}
