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

		public void UpdateExternalPerformanceName(Guid id, string name)
		{
			var sql = string.Format("UPDATE [dbo].[ExternalPerformance] SET [Name] = :name WHERE [Id] =:id");
			Session.CreateSQLQuery(sql).SetParameter("name", name).SetParameter("id", id.ToString()).ExecuteUpdate();
		}

		public void UpdateExternalPerformanceName(int externalId, int dataType, string name)
		{
			var sql = string.Format("UPDATE [dbo].[ExternalPerformance] SET [Name]=:name WHERE [ExternalId] =:externalId AND [DataType]=:dataType");
			Session.CreateSQLQuery(sql).SetParameter("name", name).SetParameter("externalId", externalId).SetParameter("dataType", dataType).ExecuteUpdate();
		}
	}
}
