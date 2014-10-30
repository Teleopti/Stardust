using System;
using Autofac;
using NHibernate;
using NHibernate.Cfg;
using Teleopti.Ccc.IocCommon.Aop.Core;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public interface ICurrentReadModelUnitOfWork
	{
		ILiteUnitOfWork Current();
	}

	public interface ILiteUnitOfWork : IDisposable
	{
		ISQLQuery CreateSqlQuery(string queryString);
		void Commit();
	}

	public interface IReadModelUnitOfWorkConfiguration
	{
		void Configure(string connectionString);
	}

	public interface IReadModelUnitOfWorkFactory
	{
		ILiteUnitOfWork NewUnitOfWork();
	}

	public class ReadModelUnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public ReadModelUnitOfWorkAttribute()
			: base(typeof(ReadModelUnitOfWorkAspect))
		{
		}
	}

	public class ReadModelUnitOfWorkAspect : IAspect
	{
		private readonly ICurrentReadModelUnitOfWork _uow;
		private readonly IReadModelUnitOfWorkFactory _factory;

		public ReadModelUnitOfWorkAspect(ICurrentReadModelUnitOfWork uow, IReadModelUnitOfWorkFactory factory)
		{
			_uow = uow;
			_factory = factory;
		}

		public void OnBeforeInvokation()
		{
			_factory.NewUnitOfWork();
		}

		public void OnAfterInvokation(Exception exception)
		{
			if (exception == null)
				_uow.Current().Commit();
			_uow.Current().Dispose();
		}
	}

	public class ReadModelUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ReadModelUnitOfWorkFactory>()
				.As<IReadModelUnitOfWorkConfiguration>()
				.As<IReadModelUnitOfWorkFactory>()
				.SingleInstance();
			builder.RegisterType<CurrentReadModelUnitOfWork>()
				.As<ICurrentReadModelUnitOfWork>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkAspect>()
				.SingleInstance();
		}
	}

	public class ReadModelUnitOfWorkFactory : IReadModelUnitOfWorkConfiguration, IReadModelUnitOfWorkFactory
	{
		private ISessionFactory _sessionFactory;

		public void Configure(string connectionString)
		{
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			//configuration.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass, "Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure");
			_sessionFactory = configuration.BuildSessionFactory();
		}

		public ILiteUnitOfWork NewUnitOfWork()
		{
			var uow = new LiteUnitOfWork(_sessionFactory.OpenSession());
			ReadModelUnitOfWorkState.UnitOfWork = uow;
			return uow;
		}
	}

	public static class ReadModelUnitOfWorkState
	{
		[ThreadStatic]
		public static ILiteUnitOfWork UnitOfWork;
	}

	public class CurrentReadModelUnitOfWork : ICurrentReadModelUnitOfWork
	{
		public ILiteUnitOfWork Current()
		{
			return ReadModelUnitOfWorkState.UnitOfWork;
		}
	}

	public class LiteUnitOfWork : ILiteUnitOfWork
	{
		private readonly ISession _session;
		private readonly ITransaction _transaction;

		public LiteUnitOfWork(ISession session)
		{
			_session = session;
			_transaction = _session.BeginTransaction();
		}

		public ISQLQuery CreateSqlQuery(string queryString)
		{
			return _session.CreateSQLQuery(queryString);
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		public void Dispose()
		{
			_transaction.Dispose();
			_session.Dispose();
		}
	}

}