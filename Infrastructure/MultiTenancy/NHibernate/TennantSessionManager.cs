using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Context;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public class TennantSessionManager : ITennantSessionFactory, ICurrentTennantSession
	{
		private ISessionFactory _sessionFactory;

		public static TennantSessionManager CreateInstanceForTest(string connectionString)
		{
			return createInstance(connectionString, "call");
		}

		public static TennantSessionManager CreateInstanceForWeb(string connectionString)
		{
			return createInstance(connectionString, "web");
		}

		private static TennantSessionManager createInstance(string connectionString, string sessionContext)
		{
			var cfg = new Configuration()
				.DataBaseIntegration(db =>
				{
					db.ConnectionString = connectionString;
					db.Dialect<MsSql2008Dialect>();
				
				});
			cfg.SetProperty(Environment.CurrentSessionContextClass, sessionContext);
			cfg.AddClass(typeof (PersonInfo));
			cfg.AddClass(typeof (PasswordPolicyForUser));

			var ret = new TennantSessionManager {_sessionFactory = cfg.BuildSessionFactory()};
			return ret;
		}

		public ISession Session()
		{
			return _sessionFactory.GetCurrentSession();
		}

		public IDisposable StartTransaction()
		{
			var session = _sessionFactory.OpenSession();
			session.BeginTransaction();
			CurrentSessionContext.Bind(session);
			return new GenericDisposable(EndTransaction);
		}

		public void EndTransaction()
		{
			_sessionFactory.GetCurrentSession().Dispose();
			CurrentSessionContext.Unbind(_sessionFactory);
		}
	}
}