using System;
using NHibernate;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public class NHibernateUnitOfWorkMatrixFactory : NHibernateUnitOfWorkFactory
    {
        private const string notStatefulSupport = "This IUnitOfWorkFactory does not support stateful IUnitOfWorks";

        protected internal NHibernateUnitOfWorkMatrixFactory(ISessionFactory sessionFactory, string connectionString)
			: base(sessionFactory, null, connectionString, new NoPersistCallbacks(), () => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging)
        {
        }

        public override IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel)
        {
            throw new NotSupportedException(notStatefulSupport);
        }
    }
}
