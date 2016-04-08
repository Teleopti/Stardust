using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	internal class NHibernateUnitOfWorkFactoryFake : NHibernateUnitOfWorkFactory
	{
		internal NHibernateUnitOfWorkFactoryFake(ISessionFactory sessFactory)
			: base(sessFactory, null, null, new NoTransactionHooks()) { }
		
		internal ISessionFactory SessFactory { get { return SessionFactory; } }
	}
}