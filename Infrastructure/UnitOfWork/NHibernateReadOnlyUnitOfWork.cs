using System;
using System.Collections.Generic;
using System.Data;
using NHibernate;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    /// <summary>
    /// A read only UnitOfWork implementation built as
    /// a wrapper around nhibernate's session
    /// </summary>
    public class NHibernateReadOnlyUnitOfWork : NHibernateUnitOfWork
    {
        private readonly IsolationLevel _readIsolationLevel;
        private ITransaction _transaction;

        protected internal NHibernateReadOnlyUnitOfWork(ISession session, 
                                                    IMessageBroker messageBroker,
                                                    IsolationLevel readIsolationLevel) : base(session,messageBroker)
        {
            _readIsolationLevel = readIsolationLevel;
        }

        protected internal override ISession Session
        {
            get
            {
                var session = base.Session;
                if (_transaction == null)
                {
                    _transaction = session.BeginTransaction(_readIsolationLevel);
                }
                return session;
            }
        }

        public override IEnumerable<IRootChangeInfo> PersistAll(IMessageBrokerModule moduleUsedForPersist)
        {
            throw new NotSupportedException("Save operations are not supported for read only unit of work.");
        }

        #region IDispose

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
            base.ReleaseManagedResources();
        }

        #endregion
    }
}