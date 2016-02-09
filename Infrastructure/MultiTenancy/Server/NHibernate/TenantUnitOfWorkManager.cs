using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkManager : ITenantUnitOfWork, ICurrentTenantSession, IDisposable
	{
		private readonly ISessionFactory _sessionFactory;

		private TenantUnitOfWorkManager(ISessionFactory sessionFactory)
		{
			_sessionFactory = sessionFactory;
		}
		
		public static TenantUnitOfWorkManager Create(string connectionString)
		{
			if(connectionString==null)
				return new TenantUnitOfWorkManager(null);

			var cfg = new Configuration()
				.DataBaseIntegration(db =>
				{
					db.ConnectionString = connectionString;
					db.Dialect<MsSql2008Dialect>();
					db.ExceptionConverter<TenantNhibernateExceptionConverter>();
				});
			cfg.SetProperty(Environment.CurrentSessionContextClass, typeof(TeleoptiSessionContext).AssemblyQualifiedName);
			//TODO: tenant - if/when tenant stuff is it's own service, we don't have to pick these one-by-one but take all assembly instead.
			cfg.AddResources(new[]
			{
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.PersonInfo.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Tenant.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Admin.TenantAdminUser.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.Authentication.CryptoKeyInfo.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.Authentication.NonceInfo.dbconf.xml"
			}, typeof (TenantUnitOfWorkManager).Assembly);

			return new TenantUnitOfWorkManager(cfg.BuildSessionFactory());
		}

		public ISession CurrentSession()
		{
			return _sessionFactory.GetCurrentSession();
		}

		public bool HasCurrentSession()
		{
			return CurrentSessionContext.HasBind(_sessionFactory);
		}

		public IDisposable EnsureUnitOfWorkIsStarted()
		{
			if (HasCurrentSession())
				return new GenericDisposable(() => {});

			var session = _sessionFactory.OpenSession();
			session.BeginTransaction();
			CurrentSessionContext.Bind(session);
			return new GenericDisposable(CommitAndDisposeCurrent);
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
			_sessionFactory.Dispose();
		}
		
	}
}