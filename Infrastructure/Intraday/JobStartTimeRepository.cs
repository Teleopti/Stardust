using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Intraday
{
	public class JobStartTimeRepository : IJobStartTimeRepository
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public JobStartTimeRepository(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Persist(Guid buId, DateTime datetime)
		{
			var hql = @"MERGE INTO JobStartTime T
						USING (
								VALUES
								(
									:BusinessUnit,
									:StartTime)
								) AS S (
									BusinessUnit,
									StartTime
								)
								ON
									T.BusinessUnit = S.BusinessUnit
								WHEN MATCHED THEN 
									UPDATE SET StartTime = S.StartTime
								WHEN NOT MATCHED THEN 
									INSERT (BusinessUnit, StartTime) values(S.BusinessUnit,S.StartTime);";
				var sqlQuery = ((NHibernateUnitOfWork)_currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(hql);
				sqlQuery.SetDateTime("StartTime", datetime);
				sqlQuery.SetGuid("BusinessUnit", @buId);
				sqlQuery.ExecuteUpdate();
		}

		public IDictionary<Guid, DateTime> LoadAll()
		{
			var result =
				((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
						@"SELECT BusinessUnit, StartTime  FROM [JobStartTime] with(nolock)")
					.SetResultTransformer(Transformers.AliasToBean(typeof(StartTimePerBu)))
					.List<StartTimePerBu>();

			return result.ToDictionary(x => x.BusinessUnit, y => y.StartTime);
		}
	}

	internal class StartTimePerBu
	{
		public Guid BusinessUnit { get; set; }
		public DateTime StartTime { get; set; }
	}
}