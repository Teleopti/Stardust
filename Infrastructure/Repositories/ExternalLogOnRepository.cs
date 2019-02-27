using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalLogOnRepository : Repository<IExternalLogOn>, IExternalLogOnRepository
	{
		public static ExternalLogOnRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ExternalLogOnRepository(currentUnitOfWork, null, null);
		}

		public static ExternalLogOnRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new ExternalLogOnRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public ExternalLogOnRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		public IList<IExternalLogOn> LoadByAcdLogOnNames(IEnumerable<string> externalLogOnNames)
		{
			return Session.CreateCriteria<ExternalLogOn>()
				.Add(Restrictions.InG(nameof(ExternalLogOn.AcdLogOnName), externalLogOnNames))
				.List<IExternalLogOn>();
		}
	}
}