using System;
using NHibernate;
using NHibernate.Stat;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
    [TestFixture]
    [Category("LongRunning")]
    public class NHibernateUnitOfWorkMatrixFactoryTest
    {
        private MockRepository mocks;
        
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void CannotCreateNormalUnitOfWork()
        {
            ISessionFactory sessFact = mocks.StrictMock<ISessionFactory>();
            IStatistics stat = mocks.StrictMock<IStatistics>();

            using(mocks.Record())
            {
                Expect.On(sessFact)
                    .Call(sessFact.Statistics)
                    .Return(stat)
                    .Repeat.Any();
                stat.IsStatisticsEnabled = true;
            }
            IUnitOfWorkFactory fact = new factory(sessFact);
            fact.CreateAndOpenUnitOfWork();
        }

        private class factory : NHibernateUnitOfWorkMatrixFactory
        {
            public factory(ISessionFactory sessionFactory) : base(sessionFactory, null)
            {
            }
        }
    }
}
