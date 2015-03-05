using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
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
			cfg.AddResources(new[]
			{
				"Teleopti.Ccc.Infrastructure.MultiTenancy.PersonInfo_OldSchema.hbm.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.PasswordPolicyForUser_OldSchema.hbm.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Tenant_OldSchema.hbm.xml"
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

		public void CancelCurrent()
		{
			var session = CurrentSessionContext.Unbind(_sessionFactory);
			if (session == null) return;
			
			session.Transaction.Rollback();
			session.Dispose();
		}

		public void CommitCurrent()
		{
			var session = CurrentSessionContext.Unbind(_sessionFactory);
			if (session == null) return;
			
			session.Transaction.Commit();
			session.Dispose();
		}

		public void Dispose()
		{
			if (_sessionFactory == null) return;
			//to end just current transaction doesn't make sense in real code, but makes testing easier
			CancelCurrent();
		}
	}
}