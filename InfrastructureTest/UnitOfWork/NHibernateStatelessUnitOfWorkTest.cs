using System;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
    [Category("LongRunning")]
    public sealed class NHibernateStatelessUnitOfWorkTest
    {
        private NHibernateStatelessUnitOfWork uow;
        private IStatelessSession session;
        private MockRepository mocks;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            session = mocks.StrictMock<IStatelessSession>();
            uow = new TestUnitOfWork(session);
        }


        /// <summary>
        /// Verifies that dispose() disposes session.
        /// </summary>
        [Test]
        public void VerifyDisposeDisposesSession()
        {
            session.Dispose();
            mocks.ReplayAll();
            uow.Dispose();
            mocks.VerifyAll();
        }


        /// <summary>
        /// The nhibernate session must not be null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SessionMustNotBeNull()
        {
            new TestUnitOfWork(null);
        }

        /// <summary>
        /// Verifies the session property is set.
        /// </summary>
        [Test]
        public void VerifySessionPropertyIsSet()
        {
            Assert.AreSame(session, ((TestUnitOfWork)uow).ShowInternalSession);
        }



        private class TestUnitOfWork : NHibernateStatelessUnitOfWork
        {
            public TestUnitOfWork(IStatelessSession mock)
                : base(mock)
            {
            }

            public IStatelessSession ShowInternalSession
            {
                get { return Session; }
            }

        }
    }
}
