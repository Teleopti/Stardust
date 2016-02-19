using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class EtlLogObjectRepository : IEtlLogObjectRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public EtlLogObjectRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public IEnumerable<LogObjectDetail> Load()
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				const string tsql = "EXEC [mart].[raptor_log_object_detail]";
				return uow.Session().CreateSQLQuery(tsql)
					.AddScalar("log_object_desc", NHibernateUtil.String)
					.AddScalar("log_object_id", NHibernateUtil.Int32)
					.AddScalar("detail_desc", NHibernateUtil.String)
					.AddScalar("proc_name", NHibernateUtil.String)
					.AddScalar("last_update", NHibernateUtil.DateTime)
					.SetResultTransformer(new AliasToBeanResultTransformer(typeof(LogObjectDetail)))
					.List<LogObjectDetail>();
			}
		}
	}
}