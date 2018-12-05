using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Wfm.Adherence.Tracer.Infrastructure
{
	public class RtaTracerConfigPersister : IRtaTracerConfigPersister
	{
		private readonly RtaTracerSessionFactory _sessionFactory;
		private readonly ICurrentDataSource _currentDataSource;

		public RtaTracerConfigPersister(RtaTracerSessionFactory sessionFactory, ICurrentDataSource currentDataSource)
		{
			_sessionFactory = sessionFactory;
			_currentDataSource = currentDataSource;
		}

		public void UpdateForTenant(string userCode)
		{
			using (var session = _sessionFactory.OpenSession())
			{
				var updated = session.CreateSQLQuery(@"UPDATE [RtaTracer].[Tracer] SET UserCode = :UserCode WHERE Tenant = :Tenant")
					.SetParameter("Tenant", _currentDataSource.CurrentName())
					.SetParameter("UserCode", userCode)
					.ExecuteUpdate();
				if (updated == 0)
					session.CreateSQLQuery(@"INSERT INTO [RtaTracer].[Tracer] (Tenant, UserCode) VALUES (:Tenant, :UserCode)")
						.SetParameter("Tenant", _currentDataSource.CurrentName())
						.SetParameter("UserCode", userCode)
						.ExecuteUpdate();
			}
		}

		public void DeleteForTenant()
		{
			using (var session = _sessionFactory.OpenSession())
				session.CreateSQLQuery(@"DELETE [RtaTracer].[Tracer] WHERE Tenant = :Tenant")
					.SetParameter("Tenant", _currentDataSource.CurrentName())
					.ExecuteUpdate();
		}

		public IEnumerable<RtaTracerConfig> ReadAll()
		{
			using (var session = _sessionFactory.OpenSession())
				return session.CreateSQLQuery(@"SELECT * FROM [RtaTracer].[Tracer]")
					.SetResultTransformer(Transformers.AliasToBean<RtaTracerConfig>())
					.List<RtaTracerConfig>();
		}
	}
}