using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests RtaStateGroupRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class RtaStateGroupRepositoryTest : RepositoryTest<IRtaStateGroup>
    {
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IRtaStateGroup CreateAggregateWithCorrectBusinessUnit()
        {
            IRtaStateGroup stateGroup = new RtaStateGroup("sg1", true, false);
            stateGroup.IsLogOutState = true;
            stateGroup.AddState("state1", "01", Guid.NewGuid());
            return stateGroup;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IRtaStateGroup loadedAggregateFromDatabase)
        {
            IRtaStateGroup org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(org.Available, loadedAggregateFromDatabase.Available);
            Assert.AreEqual(org.DefaultStateGroup, loadedAggregateFromDatabase.DefaultStateGroup);
            Assert.AreEqual(org.IsLogOutState,loadedAggregateFromDatabase.IsLogOutState);
            Assert.AreEqual(org.StateCollection.Count, loadedAggregateFromDatabase.StateCollection.Count);
        }

        protected override Repository<IRtaStateGroup> TestRepository(IUnitOfWork unitOfWork)
        {
            return new RtaStateGroupRepository(unitOfWork);
        }

        [Test]
        public void VerifyMovingStateWorks()
        {
            IRtaStateGroup stateGroup1 = CreateAggregateWithCorrectBusinessUnit();
            IRtaStateGroup stateGroup2 = CreateAggregateWithCorrectBusinessUnit();

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