#region Imports

using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.FakeData;


#endregion

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Repersents the test class for the PersonAvailabilityCurrentRotationComparer.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-15
    /// </remarks>
    [TestFixture]
    public class PersonAvailabilityCurrentRotationComparerTest
    {
        #region Private Members

        private int result;
        private IPerson person1, person2;
        private Description description1, description2;
        private PersonAvailabilityModelParent _target1, _target2;

        private readonly PersonAvailabilityCurrentRotationComparer<PersonAvailabilityModelParent, IPersonAvailability, IAvailabilityRotation> comparer =
                    new PersonAvailabilityCurrentRotationComparer
                        <PersonAvailabilityModelParent, IPersonAvailability, IAvailabilityRotation>();

        #endregion

        #region Setup and Teardown Methods

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-15
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            person1 = PersonFactory.CreatePerson();
            person2 = PersonFactory.CreatePerson();
            description1 = new Description("My 1 week rotation");
            description2 = new Description("My 2 week rotation");

            _target1 = new PersonAvailabilityModelParent(person1, null, null);
            _target2 = new PersonAvailabilityModelParent(person2, null, null);
        }

        #endregion

        #region Tests

        [Test]
        public void VerifyCompareMethodWithWithNullValues()
        {
            // Checks whether the roles are equal
            result = comparer.Compare(_target1, _target2);
            Assert.AreEqual(0, result);

            // Check with first null
            _target2 =
                PeopleAdminUtilityTestHelper.CreatePersonAvailabilityParentAdapter(new DateOnly(2000, 01, 01),
                                                                                   new Name("Kung-fu", "Panda"),
                                                                                   description1,
                                                                                   7);
            _target2.CurrentRotation = new AvailabilityRotation(description1.Name, 7);

            result = comparer.Compare(_target1, _target2);
            Assert.AreEqual(-1, result);

            // Check with second null
            result = comparer.Compare(_target2, _target1);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void VerifyAscendingDescendingValues()
        {
            // Set targets
            _target1 =
                PeopleAdminUtilityTestHelper.CreatePersonAvailabilityParentAdapter(new DateOnly(2000, 01, 01),
                                                                                   new Name("Kung-fu", "Panda"),
                                                                                   description1,
                                                                                   7);
            _target1.CurrentRotation = new AvailabilityRotation(description1.Name, 7);
            _target2 =
                PeopleAdminUtilityTestHelper.CreatePersonAvailabilityParentAdapter(new DateOnly(2000, 01, 15),
                                                                                   new Name("Dark Knight", "Bat Man"),
                                                                                   description2, 7);
            _target2.CurrentRotation = new AvailabilityRotation(description2.Name, 7);

            // Check with ascending values
            result = comparer.Compare(_target1, _target2);
            Assert.AreEqual(-1, result);

            // Check with descending values
            result = comparer.Compare(_target2, _target1);
            Assert.AreEqual(1, result);

            // Test with same value
            result = comparer.Compare(_target1, _target1);
            Assert.AreEqual(0, result);
        }

        #endregion
    }
}
