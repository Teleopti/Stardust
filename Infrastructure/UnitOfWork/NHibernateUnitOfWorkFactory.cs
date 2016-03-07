using System;
using System.Threading;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NHibernateUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly IAuditSetter _auditSettingProvider;

		private readonly ICurrentPersistCallbacks _persistCallbacks;
		private readonly Func<IMessageBrokerComposite> _messageBroker;

		protected internal NHibernateUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			IAuditSetter auditSettingProvider,
			string connectionString,
			ICurrentPersistCallbacks persistCallbacks,
			Func<IMessageBrokerComposite> messageBroker)
		{
			ConnectionString = connectionString;
			SessionContextBinder = new StaticSessionContextBinder();
			_factory = sessionFactory;
			_auditSettingProvider = auditSettingProvider;
			_persistCallbacks = persistCallbacks;
			_messageBroker = messageBroker;
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
				if(statistics.IsStatisticsEnabled)
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
			return MakeUnitOfWork(
				session,
				_messageBroker(),
				SessionContextBinder.FilterManager(session),
				SessionContextBinder.IsolationLevel(session),
				SessionContextBinder.Initiator(session)
				);
		}

		public IAuditSetter AuditSetting
		{
			get { return _auditSettingProvider; }
		}

		public string ConnectionString { get; private set; }

		public IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.Default)
		{
			return createAndOpenUnitOfWork(_messageBroker(), isolationLevel, null, QueryFilter.BusinessUnit);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IInitiatorIdentifier initiator)
		{
			return createAndOpenUnitOfWork(_messageBroker(), TransactionIsolationLevel.Default, initiator, QueryFilter.BusinessUnit);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IMessageBrokerComposite messageBroker)
		{
			return createAndOpenUnitOfWork(messageBroker, TransactionIsolationLevel.Default, null, QueryFilter.BusinessUnit);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IQueryFilter businessUnitFilter)
		{
			return createAndOpenUnitOfWork(_messageBroker(), TransactionIsolationLevel.Default, null, businessUnitFilter);
		}

		private IUnitOfWork createAndOpenUnitOfWork(IMessageBrokerComposite messageBroker, TransactionIsolationLevel isolationLevel, IInitiatorIdentifier initiator, IQueryFilter businessUnitFilter)
		{
			var businessUnitId = getBusinessUnitId();
			var session = createNhibSession(isolationLevel);

			businessUnitFilter.Enable(session, businessUnitId);
			QueryFilter.Deleted.Enable(session, null);
			QueryFilter.DeletedPeople.Enable(session, null);

			return MakeUnitOfWork(session, messageBroker, SessionContextBinder.FilterManager(session), isolationLevel, initiator);
		}

		private static Guid getBusinessUnitId()
		{
			var identity = Thread.CurrentPrincipal.Identity as ITeleoptiIdentity;
			var buId = (identity != null && identity.BusinessUnit != null)
				? identity.BusinessUnit.Id.GetValueOrDefault()
				: Guid.Empty;
			return buId;
		}

		private ISession createNhibSession(TransactionIsolationLevel isolationLevel)
		{
			var session = _factory.OpenSession(new AggregateRootInterceptor());
			session.FlushMode = FlushMode.Never;
			SessionContextBinder.Bind(session, isolationLevel);
			return session;
		}

		protected virtual IUnitOfWork MakeUnitOfWork(ISession session, IMessageBrokerComposite messaging, NHibernateFilterManager filterManager, TransactionIsolationLevel isolationLevel, IInitiatorIdentifier initiator)
		{
			return new NHibernateUnitOfWork(session,
											messaging,
											_persistCallbacks,
											filterManager,
											new SendPushMessageWhenRootAlteredService(),
											SessionContextBinder.Unbind,
											SessionContextBinder.BindInitiator,
											isolationLevel,
											initiator
											);
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}
