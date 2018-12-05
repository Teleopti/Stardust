using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Configuration.Repositories;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Infrastructure
{
    [TestFixture]
    [Category("BucketB")]
    public class RtaStateGroupRepositoryTest : RepositoryTest<IRtaStateGroup>
    {
        protected override IRtaStateGroup CreateAggregateWithCorrectBusinessUnit()
        {
            var stateGroup = new RtaStateGroup("test", true, false);
			stateGroup.AddState(Guid.NewGuid().ToString());
            return stateGroup;
        }

        protected override void VerifyAggregateGraphProperties(IRtaStateGroup loadedAggregateFromDatabase)
        {
            var org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(org.BusinessUnit.Id, loadedAggregateFromDatabase.BusinessUnit.Id);
            Assert.AreEqual(org.Available, loadedAggregateFromDatabase.Available);
            Assert.AreEqual(org.DefaultStateGroup, loadedAggregateFromDatabase.DefaultStateGroup);
            Assert.AreEqual(org.StateCollection.Count, loadedAggregateFromDatabase.StateCollection.Count);
			Assert.AreEqual(org.StateCollection[0].BusinessUnit.Id, loadedAggregateFromDatabase.StateCollection[0].BusinessUnit.Id);
        }

        protected override Repository<IRtaStateGroup> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new RtaStateGroupRepository(currentUnitOfWork);
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

            var result = new RtaStateGroupRepository(new ThisUnitOfWork(UnitOfWork)).LoadAllCompleteGraph();
            Assert.AreEqual(1,result.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[0].StateCollection));
        }

	    [Test]
	    public void ShouldNotAllowDuplicateStateCodes()
	    {
		    var stateGroup1 = new RtaStateGroup("group 1", false, true);
		    var stateGroup2 = new RtaStateGroup("group 2", false, true);
			stateGroup1.AddState("dupe", "");
			stateGroup2.AddState("dupe", "");

			PersistAndRemoveFromUnitOfWork(stateGroup1);
			Assert.Throws<ConstraintViolationException>(() => PersistAndRemoveFromUnitOfWork(stateGroup2));
		}
		
    }
}