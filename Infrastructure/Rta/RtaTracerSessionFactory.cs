using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerSessionFactory : IDisposable
	{
		private readonly ISessionFactory _sessionFactory;

		public RtaTracerSessionFactory(IConfigReader config)
		{
			_sessionFactory = buildSessionFactory(config.ConnectionString("RtaTracer"));
		}

		private ISessionFactory buildSessionFactory(string connectionString)
		{
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName);
			return configuration.BuildSessionFactory();
		}

		public ISession OpenSession()
		{
			return _sessionFactory.OpenSession();
		}

		public void Dispose()
		{
			_sessionFactory.Dispose();
		}
	}
}