using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork
{
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
			_state.Set(new LiteUnitOfWork(_sessionFactory.OpenSession()));
		}

		public void EndUnitOfWork(Exception exception)
		{
			var uow = _state.Get();
			_state.Set(null);
			if (exception == null)
				uow.Commit();
			uow.Dispose();
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