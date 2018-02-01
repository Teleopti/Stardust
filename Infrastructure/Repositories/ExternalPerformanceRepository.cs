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
		public ExternalPerformanceRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public IEnumerable<IExternalPerformance> FindAllExternalPerformances()
		{
			var businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current();
			return Session.CreateCriteria(typeof(ExternalPerformance))
				.Add(Restrictions.Eq("BusinessUnit", businessUnit))
				.AddOrder(Order.Asc("Name"))
				.List<IExternalPerformance>();
		}

		public IExternalPerformance FindExternalPerformanceByExternalId(int externalId)
		{
			var businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current();
			return Session.CreateCriteria<ExternalPerformance>()
				.Add(Restrictions.Eq("BusinessUnit", businessUnit))
				.Add(Restrictions.Eq("ExternalId", externalId))
				.UniqueResult<IExternalPerformance>();
		}

		public int GetExernalPerformanceCount()
		{
			var businessUnit = ServiceLocatorForEntity.CurrentBusinessUnit.Current();
			return Session.QueryOver<ExternalPerformance>().Where(x=>x.BusinessUnit == businessUnit).RowCount();
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
