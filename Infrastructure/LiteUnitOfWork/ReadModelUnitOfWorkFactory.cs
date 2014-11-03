﻿using System;
using NHibernate;
using NHibernate.Cfg;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkFactory : IReadModelUnitOfWorkFactory
	{
		private readonly ReadModelUnitOfWorkState _state;
		private ISessionFactory _sessionFactory;

		public ReadModelUnitOfWorkFactory(ICurrentHttpContext httpContext)
		{
			_state = new ReadModelUnitOfWorkState(httpContext);
		}

		public void Configure(string connectionString)
		{
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
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
	}
}