using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;

		protected internal AnalyticsUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			string connectionString)
		{
			ConnectionString = connectionString;
			SessionContextBinder = new StaticSessionContextBinder();
			_factory = sessionFactory;
		}

		protected ISessionContextBinder SessionContextBinder { get; set; }

		public string Name
		{
			get { return ((ISessionFactoryImplementor)_factory).Settings.SessionFactoryName; }
		}

		public ISessionFactory SessionFactory
		{
			get { return _factory; }
		}

		public long? NumberOfLiveUnitOfWorks
		{
			get
			{
				IStatistics statistics = _factory.Statistics;
				if (statistics.IsStatisticsEnabled)
					return statistics.SessionOpenCount - statistics.SessionCloseCount;
				return null;
			}
		}

		public IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork()
		{
			var nhibSession = _factory.OpenStatelessSession();
			return new NHibernateStatelessUnitOfWork(nhibSession);
		}

		public IUnitOfWork CurrentUnitOfWork()
		{
			var session = _factory.GetCurrentSession();
			return makeUnitOfWork(
				session,
				SessionContextBinder.IsolationLevel(session)
				);
		}

		public IAuditSetter AuditSetting
		{
			get { return null; }
		}

		public string ConnectionString { get; private set; }

		public IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.Default)
		{
			return createAndOpenUnitOfWork(isolationLevel);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IInitiatorIdentifier initiator)
		{
			return createAndOpenUnitOfWork(TransactionIsolationLevel.Default);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IMessageBrokerComposite messageBroker)
		{
			return createAndOpenUnitOfWork(TransactionIsolationLevel.Default);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			return createAndOpenUnitOfWork(TransactionIsolationLevel.Default);
		}

		private IUnitOfWork createAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel)
		{
			var session = createNhibSession(isolationLevel);
			return makeUnitOfWork(session, isolationLevel);
		}

		private IUnitOfWork makeUnitOfWork(ISession session, TransactionIsolationLevel isolationLevel)
		{
			return new NHibernateUnitOfWork(session,
				null,
				null,
				null,
				null,
				SessionContextBinder.Unbind,
				SessionContextBinder.BindInitiator,
				isolationLevel,
				null
				);
		}

		private ISession createNhibSession(TransactionIsolationLevel isolationLevel)
		{
			var session = _factory.OpenSession(new AggregateRootInterceptor());
			session.FlushMode = FlushMode.Never;
			SessionContextBinder.Bind(session, isolationLevel);
			return session;
		}
		
		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}