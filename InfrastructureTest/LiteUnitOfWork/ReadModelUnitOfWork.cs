using System;
using Autofac;
using NHibernate;
using NHibernate.Cfg;
using Teleopti.Ccc.IocCommon.Aop.Core;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CurrentReadModelUnitOfWork>()
				.SingleInstance()
				.As<ICurrentReadModelUnitOfWork>();
			builder.RegisterType<ReadModelUnitOfWorkAspect>()
				.SingleInstance();
		}
	}

	public class CurrentReadModelUnitOfWork : ICurrentReadModelUnitOfWork
	{
		[ThreadStatic]
		private static ILiteUnitOfWork _uow;

		public void SetCurrentSession(ISession session)
		{
			_uow = new LiteUnitOfWork(session);
		}

		public ILiteUnitOfWork Current()
		{
			return _uow;
		}
	}

	public class LiteUnitOfWork : ILiteUnitOfWork
	{
		private readonly ISession _session;

		public LiteUnitOfWork(ISession session)
		{
			_session = session;
		}

		public ISQLQuery CreateSQLQuery(string queryString)
		{
			return _session.CreateSQLQuery(queryString);
		}

		public ISession Session()
		{
			return _session;
		}
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

		public ReadModelUnitOfWorkAspect(ICurrentReadModelUnitOfWork uow)
		{
			_uow = uow;
		}

		public void OnBeforeInvokation()
		{
			var configuration = new Configuration();
			configuration.SetProperty(NHibernate.Cfg.Environment.ConnectionString, ConnectionStringHelper.ConnectionStringUsedInTests);
			configuration.SetProperty(NHibernate.Cfg.Environment.Dialect, "NHibernate.Dialect.MsSql2005Dialect");
			//configuration.SetProperty(NHibernate.Cfg.Environment.CurrentSessionContextClass, "Teleopti.Ccc.Infrastructure.NHibernateConfiguration.HybridWebSessionContext, Teleopti.Ccc.Infrastructure");
			var sessionFactory = configuration.BuildSessionFactory();
			var session = sessionFactory.OpenSession();

			((CurrentReadModelUnitOfWork)_uow).SetCurrentSession(session);
			session.BeginTransaction();
		}

		public void OnAfterInvokation(Exception exception)
		{
			var transaction = _uow.Current().Session().Transaction;
			if (exception != null)
			{
				transaction.Dispose();
				return;
			}
			transaction.Commit();
		}
	}

	public interface ICurrentReadModelUnitOfWork
	{
		ILiteUnitOfWork Current();
	}

	public interface ILiteUnitOfWork
	{
		ISQLQuery CreateSQLQuery(string queryString);
		ISession Session();
	}
}