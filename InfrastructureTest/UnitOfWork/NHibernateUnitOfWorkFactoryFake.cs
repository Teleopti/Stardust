using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	internal class NHibernateUnitOfWorkFactoryFake : NHibernateUnitOfWorkFactory
	{
		private readonly Func<IUnitOfWork> _makeUnitOfWork;

		internal NHibernateUnitOfWorkFactoryFake(ISessionFactory sessFactory)
			: base(sessFactory, null, null, new NoPersistCallbacks(), () => MessageBrokerInStateHolder.Instance) { }

		internal NHibernateUnitOfWorkFactoryFake(ISessionFactory sessFactory, 
			Func<IUnitOfWork> makeUnitOfWork,
			ISessionContextBinder sessionContextBinder
			)
			: base(sessFactory, null, null, new NoPersistCallbacks(), () => MessageBrokerInStateHolder.Instance)
		{
			_makeUnitOfWork = makeUnitOfWork;
			SessionContextBinder = sessionContextBinder;
		}

		internal ISessionFactory SessFactory { get { return SessionFactory; } }

		protected override IUnitOfWork MakeUnitOfWork(ISession session, IMessageBrokerComposite messaging, NHibernateFilterManager filterManager, TransactionIsolationLevel isolationLevel, IInitiatorIdentifier initiator) 
		{
			return _makeUnitOfWork != null ? _makeUnitOfWork.Invoke() : base.MakeUnitOfWork(session, messaging, filterManager, isolationLevel, initiator);
		}

	}
}