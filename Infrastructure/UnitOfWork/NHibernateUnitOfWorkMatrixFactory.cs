using System;
using System.Collections.Generic;
using NHibernate;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Unitofwork factory for connections against Matrix
    /// </summary>
    public class NHibernateUnitOfWorkMatrixFactory : NHibernateUnitOfWorkFactory
    {
        private const string notStatefulSupport = "This IUnitOfWorkFactory does not support stateful IUnitOfWorks";

        protected internal NHibernateUnitOfWorkMatrixFactory(ISessionFactory sessionFactory, string connectionString)
            : base(sessionFactory, null, connectionString, new List<IMessageSender>())
        {
        }

        public override IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel)
        {
            throw new NotSupportedException(notStatefulSupport);
        }
    }
}
