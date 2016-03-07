using NHibernate;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkFactory : IAnalyticsUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly StaticSessionContextBinder _sessionContextBinder;

		protected internal AnalyticsUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			string connectionString)
		{
			ConnectionString = connectionString;
			_sessionContextBinder = new StaticSessionContextBinder();
			_factory = sessionFactory;
		}
		
		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			var session = _factory.OpenStatelessSession();
			return new NHibernateStatelessUnitOfWork(session);
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			var session = _factory.GetCurrentSession();
			return makeUnitOfWork(
				session,
				_sessionContextBinder.IsolationLevel(session)
				);
		}
		
		public string ConnectionString { get; private set; }
		
		public IUnitOfWork CreateAndOpenUnitOfWork()
		{
			var session = createNhibSession(TransactionIsolationLevel.Default);
			return makeUnitOfWork(session, TransactionIsolationLevel.Default);
		}

		private ISession createNhibSession(TransactionIsolationLevel isolationLevel)
		{
			var session = _factory.OpenSession(new AggregateRootInterceptor());
			session.FlushMode = FlushMode.Never;
			_sessionContextBinder.Bind(session, isolationLevel);
			return session;
		}

		private IUnitOfWork makeUnitOfWork(ISession session, TransactionIsolationLevel isolationLevel)
		{
			return new NHibernateUnitOfWork(session,
				null,
				null,
				null,
				null,
				_sessionContextBinder.Unbind,
				_sessionContextBinder.BindInitiator,
				isolationLevel,
				null
				);
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}