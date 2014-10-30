using System;
using Autofac;
using NHibernate;
using NHibernate.Cfg;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon.Aop.Core;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public interface ICurrentReadModelUnitOfWork
	{
		ILiteUnitOfWork Current();
	}

	public interface ILiteUnitOfWork
	{
		ISQLQuery CreateSqlQuery(string queryString);
	}

	public interface IReadModelUnitOfWorkConfiguration
	{
		void Configure(string connectionString);
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
		private readonly ReadModelUnitOfWorkFactory _factory;

		public ReadModelUnitOfWorkAspect(ReadModelUnitOfWorkFactory factory)
		{
			_factory = factory;
		}

		public void OnBeforeInvokation()
		{
			_factory.StartUnitOfWork();
		}

		public void OnAfterInvokation(Exception exception)
		{
			_factory.EndUnitOfWork(exception);
		}
	}

	public class ReadModelUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ReadModelUnitOfWorkFactory>()
				.AsSelf()
				.As<IReadModelUnitOfWorkConfiguration>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkState>()
				.AsSelf()
				.As<ICurrentReadModelUnitOfWork>()
				.SingleInstance();
			builder.RegisterType<ReadModelUnitOfWorkAspect>()
				.SingleInstance();
		}
	}

	public class ReadModelUnitOfWorkFactory : IReadModelUnitOfWorkConfiguration
	{
		private readonly ReadModelUnitOfWorkState _state;
		private ISessionFactory _sessionFactory;

		public ReadModelUnitOfWorkFactory(ReadModelUnitOfWorkState state)
		{
			_state = state;
		}

		public void Configure(string connectionString)
		{
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			//configuration.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass, "Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure");
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

	public class ReadModelUnitOfWorkState : ICurrentReadModelUnitOfWork
	{
		[ThreadStatic]
		private static LiteUnitOfWork _unitOfWork;
		private readonly ICurrentHttpContext _httpContext;
		private const string itemsKey = "ReadModelUnitOfWork";

		public ReadModelUnitOfWorkState(ICurrentHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

		public LiteUnitOfWork Get()
		{
			if (_httpContext.Current() != null)
				return (LiteUnitOfWork)_httpContext.Current().Items[itemsKey];
			return _unitOfWork;
		}

		public void Set(LiteUnitOfWork uow)
		{
			if (_httpContext.Current() != null)
			{
				_httpContext.Current().Items[itemsKey] = uow;
				return;
			}
			_unitOfWork = uow;
		}

		public ILiteUnitOfWork Current()
		{
			return Get();
		}
	}

	public class LiteUnitOfWork : ILiteUnitOfWork, IDisposable
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