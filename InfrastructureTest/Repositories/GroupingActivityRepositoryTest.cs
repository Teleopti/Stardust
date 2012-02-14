using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Testclass for GroupingActivityRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class GroupingActivityRepositoryTest : RepositoryTest<GroupingActivity>
    {

        /// <summary>
        /// Runs every test implemented by repositorie's concrete implementation
        /// </summary>
        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            new GroupingActivityRepository(UnitOfWork);
        }


        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override GroupingActivity CreateAggregateWithCorrectBusinessUnit()
        {
            IList<Activity> activities = new List<Activity>();
            Activity activity1 = new Activity("1");
            Activity activity2 = new Activity("2");
            
            activities.Add(activity1);
            activities.Add(activity2);

            PersistAndRemoveFromUnitOfWork(activity1);
            PersistAndRemoveFromUnitOfWork(activity2);

            return GroupingActivityFactory.CreateGroupingActivityAggregate("myName", activities);
        }


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase"></param>
        protected override void VerifyAggregateGraphProperties(GroupingActivity loadedAggregateFromDatabase)
        {
            Assert.IsNotEmpty(loadedAggregateFromDatabase.Description.Name);
            Assert.AreEqual(2, loadedAggregateFromDatabase.ActivityCollection.Count);
        }

        protected override Repository<GroupingActivity> TestRepository(IUnitOfWork unitOfWork)
        {
            return new GroupingActivityRepository(unitOfWork);
        }
    }
}