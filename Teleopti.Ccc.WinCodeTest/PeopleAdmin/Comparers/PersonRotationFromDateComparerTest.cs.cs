#region Imports

using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.WinCodeTest.FakeData;


#endregion

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Represents the test class for the PersonRotationFromDateComparer.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-07
    /// </remarks>
    [TestFixture]
    public class PersonRotationFromDateComparerTest
    {
        #region Private Members

        private int result;
        private IPerson person1, person2;
        private IRotation rotation1, rotation2;
        private IPersonRotation personRotation1, personRotation2;
        private PersonRotationModelParent _modelParent1, _modelParent2;
        private readonly PersonRotationFromDateComparer<PersonRotationModelParent, IPersonRotation, IRotation> comparer =
            new PersonRotationFromDateComparer<PersonRotationModelParent, IPersonRotation, IRotation>();

        #endregion

        #region Setup and Teardown Methods

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-07
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            person1 = new Person();
            person2 = new Person();

            rotation1 = new Rotation("My 1 week rotation", 7);
            rotation2 = new Rotation("My 2 week rotation", 14);

            personRotation1 = PeopleAdminUtilityTestHelper.CreatePersonRotation(DateOnly.Today,
                                                                                new Name("Kung-fu", "Panda"),
                                                                                rotation1,
                                                                                person1, 1);
            personRotation2 =
                PeopleAdminUtilityTestHelper.CreatePersonRotation(DateOnly.Today.AddDays(5), new Name("Dark Knight", "Bat Man"),
                                                                  rotation2, person2, 1);
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Verifies the compare method with all null.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-07
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithAllNull()
        {
            // Creates the adopters to compare
            _modelParent1 =
                new PersonRotationModelParent(person1, null);
            _modelParent2 =
                new PersonRotationModelParent(person2, null);

            result = comparer.Compare(_modelParent1, _modelParent2);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Verifies the compare method with first null.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-07
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithFirstNull()
        {
            // Creates the adopters to compare
            _modelParent1 =
                new PersonRotationModelParent(person1, null);
            _modelParent2 =
                new PersonRotationModelParent(person2, null);
            _modelParent2.PersonRotation = personRotation2;

            result = comparer.Compare(_modelParent1, _modelParent2);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method with second null.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-07
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithSecondNull()
        {
            // Creates the adopters to compare
            _modelParent1 =
                new PersonRotationModelParent(person1, null);
            _modelParent1.PersonRotation = personRotation1;
            _modelParent2 =
                new PersonRotationModelParent(person2, null);

            result = comparer.Compare(_modelParent1, _modelParent2);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Verifies the compare method ascending.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-07
        /// </remarks>
        [Test]
        public void VerifyCompareMethodAscending()
        {
            // Creates the adopters to compare
            _modelParent1 =
                new PersonRotationModelParent(person1, null);
            _modelParent1.PersonRotation = personRotation1;
            _modelParent2 =
                new PersonRotationModelParent(person2, null);
            _modelParent2.PersonRotation = personRotation2;

            result = comparer.Compare(_modelParent1, _modelParent2);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method descending.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-07
        /// </remarks>
        [Test]
        public void VerifyCompareMethodDescending()
        {
            // Creates the adopters to compare
            _modelParent1 =
                new PersonRotationModelParent(person1, null);
            _modelParent1.PersonRotation = personRotation2;
            _modelParent2 =
                new PersonRotationModelParent(person2, null);
            _modelParent2.PersonRotation = personRotation1;

            result = comparer.Compare(_modelParent1, _modelParent2);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Verifies the compare method with all same.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-07
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithAllSame()
        {
            // Creates the adopters to compare
            _modelParent1 =
                new PersonRotationModelParent(person1, null);
            _modelParent1.PersonRotation = personRotation1;
            _modelParent2 =
                new PersonRotationModelParent(person2, null);
            _modelParent2.PersonRotation = personRotation1;

            result = comparer.Compare(_modelParent1, _modelParent2);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        #endregion
    }
}
