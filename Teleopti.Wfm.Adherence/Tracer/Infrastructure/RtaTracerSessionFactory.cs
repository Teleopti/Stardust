using System;
using NHibernate;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Wfm.Adherence.Tracer.Infrastructure
{
	public class RtaTracerSessionFactory : IDisposable
	{
		private readonly Lazy<ISessionFactory> _sessionFactory;

		public RtaTracerSessionFactory(IConfigReader config)
		{
			_sessionFactory = new Lazy<ISessionFactory>(() => buildSessionFactory(config.ConnectionString("RtaTracer")));
		}

		private static ISessionFactory buildSessionFactory(string connectionString)
		{
			var configuration = new NHibernate.Cfg.Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName);
			return configuration.BuildSessionFactory();
		}

		public ISession OpenSession()
		{
			return _sessionFactory.Value.OpenSession();
		}

		public void Dispose()
		{
			if (_sessionFactory.IsValueCreated)
				_sessionFactory.Value.Dispose();
		}
	}
}