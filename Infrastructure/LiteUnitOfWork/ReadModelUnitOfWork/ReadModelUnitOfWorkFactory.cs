using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork
{
	public class NestedReadModelUnitOfWorkException : Exception
	{
	}

	public class ReadModelUnitOfWorkFactory : IReadModelUnitOfWorkFactory
	{
		private readonly string _connectionString;
		private ReadModelUnitOfWorkState _state;
		private ISessionFactory _sessionFactory;

		public ReadModelUnitOfWorkFactory(ICurrentHttpContext httpContext, string connectionString)
		{
			_connectionString = connectionString;
			_state = new ReadModelUnitOfWorkState(httpContext);
		}

		public void Configure()
		{
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, _connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, typeof(MsSql2008Dialect).AssemblyQualifiedName);
			_sessionFactory = configuration.BuildSessionFactory();
		}

		public void StartUnitOfWork()
		{
			if (_state.Get() != null)
				throw new NestedReadModelUnitOfWorkException();
			_state.Set(new LiteUnitOfWork(_sessionFactory.OpenSession()));
		}

		public void EndUnitOfWork(Exception exception)
		{
			// the uow may be null...
			// .. if the state is disposing
			// .. or something went wrong on before/start
			// .. we think. we have seen exceptions. ;)
			var uow = _state.Get();
			_state.Set(null);
			try
			{
				if (exception == null)
					uow?.Commit();
			}
			finally
			{
				uow?.Dispose();
			}
		}

		public void Dispose()
		{
			if (_sessionFactory != null)
			{
				_sessionFactory.Dispose();
				_sessionFactory = null;
			}
			_state = null;
		}
	}
}