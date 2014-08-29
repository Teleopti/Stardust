using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using IMessageSender = Teleopti.Ccc.Infrastructure.UnitOfWork.IMessageSender;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	internal class NHibernateUnitOfWorkFactoryFake : NHibernateUnitOfWorkFactory
	{
		private readonly Func<IUnitOfWork> _makeUnitOfWork;

		internal NHibernateUnitOfWorkFactoryFake(ISessionFactory sessFactory)
			: base(sessFactory, null, null, new List<IMessageSender>()) { }

		internal NHibernateUnitOfWorkFactoryFake(ISessionFactory sessFactory, 
			Func<IUnitOfWork> makeUnitOfWork,
			ISessionContextBinder sessionContextBinder
			)
			: base(sessFactory, null, null, new List<IMessageSender>())
		{
			_makeUnitOfWork = makeUnitOfWork;
			SessionContextBinder = sessionContextBinder;
		}

		internal ISessionFactory SessFactory { get { return SessionFactory; } }

		protected override IUnitOfWork MakeUnitOfWork(ISession session, IMessageBroker messaging, NHibernateFilterManager filterManager, TransactionIsolationLevel isolationLevel, IInitiatorIdentifier initiator) 
		{
			return _makeUnitOfWork != null ? _makeUnitOfWork.Invoke() : base.MakeUnitOfWork(session, messaging, filterManager, isolationLevel, initiator);
		}

	}
}