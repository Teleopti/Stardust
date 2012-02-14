using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	internal class NHibernateUnitOfWorkFactoryFake : NHibernateUnitOfWorkFactory
	{
		private readonly Func<IUnitOfWork> _makeUnitOfWork;

		internal NHibernateUnitOfWorkFactoryFake(ISessionFactory sessFactory)
			: base(sessFactory, null, new List<IDenormalizer>()) { }

		internal NHibernateUnitOfWorkFactoryFake(ISessionFactory sessFactory, 
			Func<IUnitOfWork> makeUnitOfWork,
			ISessionContextBinder sessionContextBinder
			)
			: base(sessFactory, null, new List<IDenormalizer>())
		{
			_makeUnitOfWork = makeUnitOfWork;
			SessionContextBinder = sessionContextBinder;
		}

		internal ISessionFactory SessFactory { get { return SessionFactory; } }

		protected override IUnitOfWork MakeUnitOfWork(ISession session, IMessageBroker messaging, NHibernateFilterManager filterManager) {
			return _makeUnitOfWork != null ? _makeUnitOfWork.Invoke() : base.MakeUnitOfWork(session, messaging, filterManager);
		}

	}
}