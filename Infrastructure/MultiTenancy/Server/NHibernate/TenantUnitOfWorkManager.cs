using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkManager : ITenantUnitOfWorkManager, ICurrentTenantSession, IDisposable
	{
		private ISessionFactory _sessionFactory;

		private TenantUnitOfWorkManager()
		{
		}

		public static TenantUnitOfWorkManager CreateInstanceForTest(string connectionString)
		{
			return createInstance(connectionString, "call");
		}

		public static TenantUnitOfWorkManager CreateInstanceForWeb(string connectionString)
		{
			return createInstance(connectionString, "web");
		}

		private static TenantUnitOfWorkManager createInstance(string connectionString, string sessionContext)
		{
			if(connectionString==null)
				return new TenantUnitOfWorkManager();

			var cfg = new Configuration()
				.DataBaseIntegration(db =>
				{
					db.ConnectionString = connectionString;
					db.Dialect<MsSql2008Dialect>();
				});
			cfg.SetProperty(Environment.CurrentSessionContextClass, sessionContext);
			//TODO: tenant - if/when tenant stuff is it's own service, we don't have to pick these one-by-one but take all assembly instead.
			//TODO: tenant - remove "oldschema" stuff when we're done.
			cfg.AddResources(new[]
			{
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.PersonInfo_OldSchema.hbm.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.PersonInfo.hbm.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Tenant_OldSchema.hbm.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Tenant.hbm.xml"
			}, typeof (TenantUnitOfWorkManager).Assembly);

			var ret = new TenantUnitOfWorkManager {_sessionFactory = cfg.BuildSessionFactory()};
			return ret;
		}

		public ISession CurrentSession()
		{
			if (!HasCurrentSession())
				startTransaction();
			return _sessionFactory.GetCurrentSession();
		}

		public bool HasCurrentSession()
		{
			return CurrentSessionContext.HasBind(_sessionFactory);
		}

		private void startTransaction()
		{
			var session = _sessionFactory.OpenSession();
			session.BeginTransaction();
			CurrentSessionContext.Bind(session);
		}

		public void CancelAndDisposeCurrent()
		{
			var session = CurrentSessionContext.Unbind(_sessionFactory);
			if (session == null) return;
			try
			{
				session.Transaction.Rollback();
			}
			finally
			{
				session.Dispose();
			}
		}

		public void CommitAndDisposeCurrent()
		{
			var session = CurrentSessionContext.Unbind(_sessionFactory);
			if (session == null) return;
			try
			{
				session.Transaction.Commit();
			}
			finally
			{
				session.Dispose();
			}
		}

		public void Dispose()
		{
			if (_sessionFactory == null) return;
			//to end just current transaction doesn't make sense in real code, but makes testing easier
			CancelAndDisposeCurrent();
		}
	}
}