using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Impl;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkManager : ITenantUnitOfWork, ICurrentTenantSession, IDisposable
	{
		private readonly Lazy<ISessionFactory> _sessionFactory;
		private readonly TenantSessionContext _context;

		private TenantUnitOfWorkManager(TenantSessionContext context, Lazy<ISessionFactory> sessionFactory)
		{
			_context = context;
			_sessionFactory = sessionFactory;
		}

		public static TenantUnitOfWorkManager Create(string connectionString)
		{
			if(connectionString==null)
				return new TenantUnitOfWorkManager(new TenantSessionContext(""), null);

			var cfg = new Configuration()
				.DataBaseIntegration(db =>
				{
					db.ConnectionString = connectionString;
					db.Dialect<MsSql2008Dialect>();
					db.ExceptionConverter<TenantNhibernateExceptionConverter>();
					db.Driver<ResilientSql2008ClientDriver>();
				});
			//TODO: tenant - if/when tenant stuff is it's own service, we don't have to pick these one-by-one but take all assembly instead.
			cfg.AddResources(new[]
			{
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.PersonInfo.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Admin.TenantAdminUser.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.Authentication.CryptoKeyInfo.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.Authentication.NonceInfo.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.ExternalApplicationAccess.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.TenantAudit.dbconf.xml"
			}, typeof (TenantUnitOfWorkManager).Assembly);
			cfg.AddResources(new[]
			{
				//"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Tenant.dbconf.xml",
				"Teleopti.Wfm.Shared.Tenant.Tenant.dbconf.xml",
			}, typeof (Tenant).Assembly);

			return new TenantUnitOfWorkManager(
				new TenantSessionContext(connectionString),
				new Lazy<ISessionFactory>(() => cfg.BuildSessionFactory())
				);
		}

		public ISession CurrentSession()
		{
			var session = _context.Get();
			// maybe better to return null..
			// but mimic nhibernate session context for now
			if (session == null)
				throw new HibernateException("No session bound to the current context");
			return session;
		}

		public Guid CurrentSessionId()
		{
			var sessionImpl = CurrentSession() as SessionImpl;
			return sessionImpl.SessionId;
		}

		public string GetSessionIntent()
		{
			return string.Empty;
		}

		public bool HasCurrentSession()
		{
			return _context.Get() != null;
		}

		public IDisposable EnsureUnitOfWorkIsStarted()
		{
			if (HasCurrentSession())
				return new GenericDisposable(() => {});

			var session = _sessionFactory.Value.OpenSession();
			session.BeginTransaction();
			_context.Set(session);
			return new GenericDisposable(CommitAndDisposeCurrent);
		}

		public void CancelAndDisposeCurrent()
		{
			var session = _context.Get();
			_context.Clear();
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
			var session = _context.Get();
			_context.Clear();
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
			if (_sessionFactory == null || !_sessionFactory.IsValueCreated) return;
			//to end just current transaction doesn't make sense in real code, but makes testing easier
			CancelAndDisposeCurrent();
			if (_sessionFactory.IsValueCreated)
			{
				_sessionFactory.Value.Dispose();
			}
		}
	}
}