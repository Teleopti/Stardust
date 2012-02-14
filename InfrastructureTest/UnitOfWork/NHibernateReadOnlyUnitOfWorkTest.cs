using System;
using System.Data;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    /// <summary>
    /// Tests for nhibernate's implementation of IUnitOfWork
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    [Category("LongRunning")]
    public sealed class NHibernateReadOnlyUnitOfWorkTest
    {
        private IUnitOfWork uow;
        private ISession session;
        private MockRepository mocks;
        private IMessageBroker messageBroker;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            session = mocks.CreateMock<ISession>();
            messageBroker = mocks.CreateMock<IMessageBroker>();
            uow = new TestUnitOfWork(session, messageBroker, IsolationLevel.Snapshot);
        }

        /// <summary>
        /// Verifies that dispose() disposes session.
        /// </summary>
        [Test]
        public void VerifyDisposeDisposesSession()
        {
            ITransaction transaction = mocks.CreateMock<ITransaction>();
            Expect.Call(session.BeginTransaction(IsolationLevel.Snapshot)).Return(transaction);
            transaction.Rollback();
            transaction.Dispose();
            session.Dispose();
            mocks.ReplayAll();
            Assert.IsNotNull(((TestUnitOfWork)uow).InternalSession);
            uow.Dispose();
            mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies that PersistAll not works for read only uow.
        /// </summary>
        [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyPersistAllNotSupported()
        {
            uow.PersistAll();
        }

        /// <summary>
        /// Verifies that PersistAll not works for read only uow.
        /// </summary>
        [Test, ExpectedException(typeof(NotSupportedException))]
        public void VerifyPersistAllWithArgumentNotSupported()
        {
            uow.PersistAll(null);
        }

        private class TestUnitOfWork : NHibernateReadOnlyUnitOfWork
        {
            public TestUnitOfWork(ISession mock, IMessageBroker messageBroker, IsolationLevel readIsolationLevel) : base(mock, messageBroker,readIsolationLevel)
            {
            }

            public ISession InternalSession
            {
                get { return Session; }
            }
        }
    }
}