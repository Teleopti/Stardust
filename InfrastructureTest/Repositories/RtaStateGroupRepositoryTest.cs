using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class RtaStateGroupRepositoryTest : RepositoryTest<IRtaStateGroup>
    {
        protected override IRtaStateGroup CreateAggregateWithCorrectBusinessUnit()
        {
            var stateGroup = new RtaStateGroup("test", true, false)
            {
	            IsLogOutState = true
            };
	        stateGroup.AddState("state1", "01", Guid.NewGuid());
            return stateGroup;
        }

        protected override void VerifyAggregateGraphProperties(IRtaStateGroup loadedAggregateFromDatabase)
        {
            var org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(org.BusinessUnit.Id, loadedAggregateFromDatabase.BusinessUnit.Id);
            Assert.AreEqual(org.Available, loadedAggregateFromDatabase.Available);
            Assert.AreEqual(org.DefaultStateGroup, loadedAggregateFromDatabase.DefaultStateGroup);
            Assert.AreEqual(org.IsLogOutState,loadedAggregateFromDatabase.IsLogOutState);
            Assert.AreEqual(org.StateCollection.Count, loadedAggregateFromDatabase.StateCollection.Count);
			Assert.AreEqual(org.StateCollection[0].BusinessUnit.Id, loadedAggregateFromDatabase.StateCollection[0].BusinessUnit.Id);
        }

        protected override Repository<IRtaStateGroup> TestRepository(IUnitOfWork unitOfWork)
        {
            return new RtaStateGroupRepository(unitOfWork);
        }

        [Test]
        public void VerifyMovingStateWorks()
        {
            var stateGroup1 = CreateAggregateWithCorrectBusinessUnit();
            var stateGroup2 = CreateAggregateWithCorrectBusinessUnit();

            PersistAndRemoveFromUnitOfWork(stateGroup1);
            PersistAndRemoveFromUnitOfWork(stateGroup2);

            stateGroup1.MoveStateTo(stateGroup2, stateGroup1.StateCollection[0]);

            PersistAndRemoveFromUnitOfWork(stateGroup1);
            PersistAndRemoveFromUnitOfWork(stateGroup2);

            Assert.AreEqual(0, stateGroup1.StateCollection.Count);
            Assert.AreEqual(2, stateGroup2.StateCollection.Count);
        }

        [Test]
        public void VerifyLoadAllCompleteGraph()
        {
            var stateGroup = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(stateGroup);

            var result = new RtaStateGroupRepository(UnitOfWork).LoadAllCompleteGraph();
            Assert.AreEqual(1,result.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].StateCollection));
        }

    }
}