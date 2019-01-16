using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Test for QueueSourceRepository
    /// </summary>
    [TestFixture]
    [Category("BucketB")]
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

        [Test]
        public void MustBeUniqueMartAggOrgData()
        {
            var qs1 = new QueueSource {QueueMartId = 1, QueueAggId = "2", QueueOriginalId = "3", DataSourceId = 4};
            var qs2 = new QueueSource {QueueMartId = 1, QueueAggId = "2", QueueOriginalId = "3", DataSourceId = 4};
            PersistAndRemoveFromUnitOfWork(qs1);
            Assert.Throws<ConstraintViolationException>(() => PersistAndRemoveFromUnitOfWork(qs2));
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
            PersistAndRemoveFromUnitOfWork(new QueueSource("Q1", "Queue1", "1"));
            PersistAndRemoveFromUnitOfWork(new QueueSource("Q2", "Queue2", "2"));
            PersistAndRemoveFromUnitOfWork(new QueueSource("Q3", "Queue3", "3"));


            _target = new QueueSourceRepository(UnitOfWork);
            IList<IQueueSource> queueSources = _target.LoadAll().ToList();
            Assert.IsNotNull(queueSources);
            Assert.AreEqual(queueSources.Count, 3);

        }
        
        protected override Repository<IQueueSource> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new QueueSourceRepository(currentUnitOfWork.Current());
        }
    }
}