﻿using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkManager : ITenantUnitOfWork, ICurrentTenantSession, IDisposable
	{
		private ISessionFactory _sessionFactory;

		private TenantUnitOfWorkManager()
		{
		}

		public static TenantUnitOfWorkManager CreateInstanceForHostsWithOneUser(string connectionString)
		{
			return createInstance(connectionString, "call");
		}

		public static TenantUnitOfWorkManager CreateInstanceForWeb(string connectionString)
		{
			return createInstance(connectionString, "web");
		}

		public static TenantUnitOfWorkManager CreateInstanceForThread(string connectionString)
		{
			return createInstance(connectionString, "thread_static");
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
					db.ExceptionConverter<TenantNhibernateExceptionConverter>();
				});
			cfg.SetProperty(Environment.CurrentSessionContextClass, sessionContext);
			//TODO: tenant - if/when tenant stuff is it's own service, we don't have to pick these one-by-one but take all assembly instead.
			cfg.AddResources(new[]
			{
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.PersonInfo.dbconf.xml",
				"Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Tenant.dbconf.xml"
			}, typeof (TenantUnitOfWorkManager).Assembly);

			var ret = new TenantUnitOfWorkManager {_sessionFactory = cfg.BuildSessionFactory()};
			return ret;
		}

		public ISession CurrentSession()
		{
			return _sessionFactory.GetCurrentSession();
		}

		public bool HasCurrentSession()
		{
			return CurrentSessionContext.HasBind(_sessionFactory);
		}

		public IDisposable Start()
		{
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
			_sessionFactory = null;
		}
	}
}