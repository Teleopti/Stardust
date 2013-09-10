using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.DomainTest.Helper;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for GroupingActivity Class
    /// </summary>
    [TestFixture]
    public class GroupingActivityTest
    {
        private GroupingActivity myGroupingActivity;

        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            myGroupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity("myName");
        }

        /// <summary>
        /// Test to make new GroupingActivity and checks the default Values
        /// </summary>
        [Test]
        public void CanCreateAndLoadDefaults()
        {
            Assert.AreEqual("myName", myGroupingActivity.Description.Name);
            Assert.AreEqual("myName", myGroupingActivity.Name);
            Assert.IsNull(myGroupingActivity.UpdatedBy);
            Assert.IsNull(myGroupingActivity.UpdatedOn);
            Assert.IsNull(myGroupingActivity.Id);
            Assert.IsNull(myGroupingActivity.Version);
        }

        [Test]
        public void VerifyBelongsToBusinessUnit()
        {
            Assert.IsFalse(myGroupingActivity is IBelongsToBusinessUnit);
        }


        /// <summary>
        /// Make sure the constructor does not take null as argument
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotSetNameToNull()
        {
            myGroupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity(null);
        }

        /// <summary>
        /// Make sure the constructor does not take empty string as argument
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotSetEmptyName()
        {
            String myName = String.Empty;
            myGroupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity(myName);
        }

        /// <summary>
        /// Can change name if not empty or null
        /// </summary>
        [Test]
        public void CanChangeName()
        {
            String myName = "name";
            String myNewName = "newName";
            myGroupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity(myName);
            myGroupingActivity.Description = new Description(myNewName);
            Assert.AreEqual(myGroupingActivity.Description.Name, myNewName);
        }

        /// <summary>
        /// Test protectedConstructorWorks
        /// </summary>
        [Test]
        public void TestProtectedConstructorForNHibernate()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(myGroupingActivity.GetType()));
        }

        /// <summary>
        /// Verifies that activities can be added.
        /// </summary>
        [Test]
        public void VerifyActivitiesCanBeAdded()
        {
            int countBefore = myGroupingActivity.ActivityCollection.Count;
            Activity testActivity1 = ActivityFactory.CreateActivity("dummyActivity1");
            Activity testActivity2 = ActivityFactory.CreateActivity("dummyActivity2");

            myGroupingActivity.AddActivity(testActivity1);
            myGroupingActivity.AddActivity(testActivity2);

            Assert.Contains(testActivity1, myGroupingActivity.ActivityCollection);
            Assert.Contains(testActivity1, myGroupingActivity.ActivityCollection);
            Assert.AreEqual(countBefore + 2, myGroupingActivity.ActivityCollection.Count);
        }

        /// <summary>
        /// Duplicate activity instances should be ignored when added to list.
        /// </summary>
        [Test]
        public void DoNotDuplicateActivityInstancesWhenAddedToList()
        {
            int countBefore = myGroupingActivity.ActivityCollection.Count;
            int lastPosBeforeAdded = countBefore - 1;
            Activity testActivity = ActivityFactory.CreateActivity("dummyActivity1");

            myGroupingActivity.AddActivity(testActivity);
            myGroupingActivity.AddActivity(testActivity);
            myGroupingActivity.AddActivity(testActivity);

            Assert.AreSame(testActivity, myGroupingActivity.ActivityCollection[lastPosBeforeAdded + 1]);
            Assert.AreEqual(countBefore + 1, myGroupingActivity.ActivityCollection.Count);
        }

        /// <summary>
        /// Null activities are not allowed.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void NullActivitiesAreNotAllowed()
        {
            myGroupingActivity.AddActivity(null);
        }

        [Test]
        public void VerifySetDeleted()
        {
            myGroupingActivity.SetDeleted();
            Assert.IsTrue(myGroupingActivity.IsDeleted);
        }
    }
}