using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Test for QueueSourceRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class QueueSourceRepositoryTest : RepositoryTest<IQueueSource>
    {
        private QueueSourceRepository _target;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }


        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IQueueSource CreateAggregateWithCorrectBusinessUnit()
        {
            return QueueSourceFactory.CreateQueueSource();
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        /// 
        protected override void VerifyAggregateGraphProperties(IQueueSource loadedAggregateFromDatabase)
        {
            IQueueSource queueSource = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(queueSource.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(queueSource.Description, loadedAggregateFromDatabase.Description);
        }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            new QueueSourceRepository(UnitOfWork);
        }


        /// <summary>
        /// Determines whether this instance [can load all queues].
        /// </summary>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-11-14
        /// </remarks>
        [Test]
        public void CanLoadAllQueues()
        {
            PersistAndRemoveFromUnitOfWork(new QueueSource("Q1", "Queue1", 1));
            PersistAndRemoveFromUnitOfWork(new QueueSource("Q2", "Queue2", 1));
            PersistAndRemoveFromUnitOfWork(new QueueSource("Q3", "Queue3", 1));


            _target = new QueueSourceRepository(UnitOfWork);
            IList<IQueueSource> queueSources = _target.LoadAll();
            Assert.IsNotNull(queueSources);
            Assert.AreEqual(queueSources.Count, 3);

        }

        [Test]
        public void CanLoadAllQueuesSourcesDistinctLogName()
        {
            var qs1 = new QueueSource { LogObjectName = "4", DataSourceId = 4 };
            var qs2 = new QueueSource { LogObjectName = "8", DataSourceId = 8 };
            PersistAndRemoveFromUnitOfWork(qs1);
            PersistAndRemoveFromUnitOfWork(qs2);

            var target = new QueueSourceRepository(UnitOfWork);
            var queueSources = target.GetDistinctLogItemName();
            queueSources.Count.Should().Be.EqualTo(2);
            queueSources[4].Should().Be.EqualTo("4");
            queueSources[8].Should().Be.EqualTo("8");
        }

        [Test]
        public void ShouldNotCrashIfDifferentNameForSameId()
        {
            var qs1 = new QueueSource { LogObjectName = "4", DataSourceId = 4 };
            var qs2 = new QueueSource { LogObjectName = "8", DataSourceId = 4 };
            PersistAndRemoveFromUnitOfWork(qs1);
            PersistAndRemoveFromUnitOfWork(qs2);

            var target = new QueueSourceRepository(UnitOfWork);
            var queueSources = target.GetDistinctLogItemName();
            queueSources.Count.Should().Be.EqualTo(1);
            //don't matter if 4 or 8 is used - shouldn't crash, thats all
        }

        protected override Repository<IQueueSource> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new QueueSourceRepository(currentUnitOfWork.Current());
        }
    }
}