using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for GroupingAbsence Class
    /// </summary>
    [TestFixture]
    public class GroupingAbsenceTest
    {
        private GroupingAbsence myGroupingAbsence;

        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            myGroupingAbsence = GroupingAbsenceFactory.CreateSimpleGroupingAbsence("myName");
        }

        /// <summary>
        /// Test to make new GroupingAbsence and checks the default Values
        /// </summary>
        [Test]
        public void CanCreateAndLoadDefaults()
        {
            Assert.AreEqual("myName", myGroupingAbsence.Description.Name);
            Assert.IsNull(myGroupingAbsence.UpdatedBy);
            Assert.IsNull(myGroupingAbsence.UpdatedOn);
            Assert.IsNull(myGroupingAbsence.Id);
            Assert.IsNull(myGroupingAbsence.Version);
        }

        /// <summary>
        /// Make sure the constructor does not take null as argument
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotSetNameToNull()
        {
            myGroupingAbsence = GroupingAbsenceFactory.CreateSimpleGroupingAbsence(null);
        }

        /// <summary>
        /// Make sure the constructor does not take empty string as argument
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotSetEmptyName()
        {
            String myName = String.Empty;
            myGroupingAbsence = GroupingAbsenceFactory.CreateSimpleGroupingAbsence(myName);
        }

        [Test]
        public void VerifyCanGetSetName()
        {
            string shortName = "shortName";
            string name = "name";

            myGroupingAbsence.ShortName = shortName;
            myGroupingAbsence.Name = name;

            Assert.AreEqual(shortName, myGroupingAbsence.ShortName);
            Assert.AreEqual(name, myGroupingAbsence.Name);
        }

        [Test]
        public void VerifyBelongsToBusinessUnit()
        {
            Assert.IsFalse(myGroupingAbsence is IBelongsToBusinessUnit);
        }


        /// <summary>
        /// Can change name if not empty or null
        /// </summary>
        [Test]
        public void CanChangeName()
        {
            String myName = "name";
            String myNewName = "newName";
            myGroupingAbsence = GroupingAbsenceFactory.CreateSimpleGroupingAbsence(myName);
            myGroupingAbsence.Description = new Description(myNewName);
            Assert.AreEqual(myGroupingAbsence.Description.Name, myNewName);
        }

        /// <summary>
        /// Test protectedConstructorWorks
        /// </summary>
        [Test]
        public void TestProtectedConstructorForNHibernate()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(myGroupingAbsence.GetType()));
        }

        /// <summary>
        /// Verifies that absence types can be added.
        /// </summary>
        [Test]
        public void VerifyAbsencesCanBeAdded()
        {
            int countBefore = myGroupingAbsence.AbsenceCollection.Count;
            IAbsence testAbsence1 = AbsenceFactory.CreateAbsence("dummyAbsence1");
            IAbsence testAbsence2 = AbsenceFactory.CreateAbsence("dummyAbsence2");

            myGroupingAbsence.AddAbsence(testAbsence1);
            myGroupingAbsence.AddAbsence(testAbsence2);

            Assert.Contains(testAbsence1, myGroupingAbsence.AbsenceCollection);
            Assert.Contains(testAbsence1, myGroupingAbsence.AbsenceCollection);
            Assert.AreEqual(countBefore + 2, myGroupingAbsence.AbsenceCollection.Count);
        }

        /// <summary>
        /// Duplicate absence instances should be ignored when added to list.
        /// </summary>
        [Test]
        public void DoNotDuplicateAbsenceInstancesWhenAddedToList()
        {
            int countBefore = myGroupingAbsence.AbsenceCollection.Count;
            int lastPosBeforeAdded = countBefore - 1;
            IAbsence testAbsence = AbsenceFactory.CreateAbsence("dummyAbsence1");

            myGroupingAbsence.AddAbsence(testAbsence);
            myGroupingAbsence.AddAbsence(testAbsence);
            myGroupingAbsence.AddAbsence(testAbsence);

            Assert.AreSame(testAbsence, myGroupingAbsence.AbsenceCollection[lastPosBeforeAdded + 1]);
            Assert.AreEqual(countBefore + 1, myGroupingAbsence.AbsenceCollection.Count);
        }

        /// <summary>
        /// Null absence types are not allowed.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void NullAbsencesAreNotAllowed()
        {
            myGroupingAbsence.AddAbsence(null);
        }

        [Test]
        public void VerifySetDeleted()
        {
            myGroupingAbsence.SetDeleted();
            Assert.IsTrue(myGroupingAbsence.IsDeleted);
        }


    }
}