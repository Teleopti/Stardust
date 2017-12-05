using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalPerformanceRepository : Repository<IExternalPerformance>, IExternalPerformanceRepository
	{
#pragma warning disable 618
		public ExternalPerformanceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
#pragma warning restore 618
		{
		}

		public ExternalPerformanceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IEnumerable<IExternalPerformance> FindAllExternalPerformances()
		{
			return Session.CreateCriteria(typeof(ExternalPerformance))
				.AddOrder(Order.Asc("Name"))
				.List<IExternalPerformance>();
		}

		public IExternalPerformance FindExternalPerformanceByExternalId(int externalId)
		{
			return Session.CreateCriteria<ExternalPerformance>()
				.Add(Restrictions.Eq("ExternalId", externalId))
				.UniqueResult<IExternalPerformance>();
		}

		public int GetExernalPerformanceCount()
		{
			return Session.QueryOver<ExternalPerformance>().RowCount();
		}

		public bool Equals(IEntity other)
		{
			throw new NotImplementedException();
		}

		public Guid? Id { get; }
		public void SetId(Guid? newId)
		{
			throw new NotImplementedException();
		}

		public void ClearId()
		{
			throw new NotImplementedException();
		}
	}
}
