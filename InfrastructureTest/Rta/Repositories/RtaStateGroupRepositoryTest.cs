using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class RtaStateGroupRepositoryTest : RepositoryTest<IRtaStateGroup>
    {
		private readonly PlatformTypeInjector platformTypeInjector = new PlatformTypeInjector();

        protected override IRtaStateGroup CreateAggregateWithCorrectBusinessUnit()
        {
            var stateGroup = new RtaStateGroup("test", true, false);
			var platformTypeId = Guid.NewGuid().ToString();
			stateGroup.AddState(platformTypeInjector.Inject("01", platformTypeId), "state1");
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
		    var platformTypeId = Guid.NewGuid().ToString();
			stateGroup1.AddState(platformTypeInjector.Inject("dupe ", platformTypeId), "");
			stateGroup2.AddState(platformTypeInjector.Inject("dupe ", platformTypeId), "");

			PersistAndRemoveFromUnitOfWork(stateGroup1);
			Assert.Throws<ConstraintViolationException>(() => PersistAndRemoveFromUnitOfWork(stateGroup2));
		}

		[Test]
		public void ShouldAllowSameStateCodeFromDifferentPlatforms()
		{
			var stateGroup1 = new RtaStateGroup("group 1", false, true);
			var stateGroup2 = new RtaStateGroup("group 2", false, true);
			stateGroup1.AddState(platformTypeInjector.Inject("same", Guid.NewGuid().ToString()), "");
			stateGroup2.AddState(platformTypeInjector.Inject("same", Guid.NewGuid().ToString()), "");

			PersistAndRemoveFromUnitOfWork(stateGroup1);
			PersistAndRemoveFromUnitOfWork(stateGroup2);
		}

    }

	[TestFixture]
	public class RtaStateGroupRepositoryTestWithoutTransaction : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldHardDelete()
		{
			var stateGroup = new RtaStateGroup("group 1", false, true);
			var target = new RtaStateGroupRepository(new ThisUnitOfWork(UnitOfWork));
			target.Add(stateGroup);
			UnitOfWork.PersistAll();

			target.Remove(stateGroup);
			UnitOfWork.PersistAll();

			UnitOfWork.DisableFilter(QueryFilter.Deleted);
			target.LoadAll().Should().Be.Empty();
		}
	}
}