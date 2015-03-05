using System;
using System.Collections.Generic;
using System.Threading;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	/// <summary>
	/// Factory for UnitOfWork. 
	/// Implemented using nhibernate's ISessionFactory.
	/// </summary>
	public class NHibernateUnitOfWorkFactory : IUnitOfWorkFactory
	{
		private readonly ISessionFactory _factory;
		private readonly IAuditSetter _auditSettingProvider;

		private readonly IEnumerable<IMessageSender> _messageSenders;
		private readonly Func<IMessageBrokerComposite> _messageBroker;

		protected internal NHibernateUnitOfWorkFactory(
			ISessionFactory sessionFactory,
			IAuditSetter auditSettingProvider,
			string connectionString,
			IEnumerable<IMessageSender> messageSenders,
			Func<IMessageBrokerComposite> messageBroker)
		{
			ConnectionString = connectionString;
			SessionContextBinder = new StaticSessionContextBinder();
			InParameter.NotNull("sessionFactory", sessionFactory);
			sessionFactory.Statistics.IsStatisticsEnabled = true;
			_factory = sessionFactory;
			_auditSettingProvider = auditSettingProvider;
			_messageSenders = messageSenders;
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

		protected virtual IUnitOfWork MakeUnitOfWork(ISession session, IMessageBrokerComposite messaging, NHibernateFilterManager filterManager, TransactionIsolationLevel isolationLevel, IInitiatorIdentifier initiator)
		{
			return new NHibernateUnitOfWork(session,
			                                messaging,
											_messageSenders,
											filterManager,
											new SendPushMessageWhenRootAlteredService(),
											SessionContextBinder.Unbind,
											SessionContextBinder.BindInitiator,
											isolationLevel,
											initiator
											);
		}

		public virtual IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.Default)
		{
			return CreateAndOpenUnitOfWork(_messageBroker(), isolationLevel, null);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IInitiatorIdentifier initiator)
		{
			return CreateAndOpenUnitOfWork(_messageBroker(), TransactionIsolationLevel.Default, initiator);
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IMessageBrokerComposite messageBroker, TransactionIsolationLevel isolationLevel, IInitiatorIdentifier initiator)
		{
			var identity = Thread.CurrentPrincipal.Identity as ITeleoptiIdentity;
			var buId = (identity !=null && identity.BusinessUnit!=null) ? identity.BusinessUnit.Id.GetValueOrDefault() : Guid.Empty;
			var interceptor = new AggregateRootInterceptor();
			var nhibSession = createNhibSession(interceptor, buId, isolationLevel);

			var nhUow = MakeUnitOfWork(nhibSession, messageBroker, SessionContextBinder.FilterManager(nhibSession), isolationLevel, initiator);
			return nhUow;
		}

		public IUnitOfWork CreateAndOpenUnitOfWork(IAggregateRoot reassociate)
		{
			var uow = CreateAndOpenUnitOfWork();
			uow.Reassociate(reassociate);
			return uow;
		}

		private ISession createNhibSession(IInterceptor interceptor, Guid buId, TransactionIsolationLevel isolationLevel)
		{
			var nhibSession = _factory.OpenSession(interceptor);
			nhibSession.FlushMode = FlushMode.Never;
			nhibSession.EnableFilter("businessUnitFilter").SetParameter("businessUnitParameter", buId);
			nhibSession.EnableFilter("deletedFlagFilter");
			nhibSession.EnableFilter("deletedPeopleFilter");
			SessionContextBinder.Bind(nhibSession, isolationLevel);
			return nhibSession;
		}

		public void Dispose()
		{
			_factory.Dispose();
		}
	}
}
