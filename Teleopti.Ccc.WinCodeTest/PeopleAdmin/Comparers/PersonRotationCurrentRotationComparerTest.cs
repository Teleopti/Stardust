#region Imports

using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// The test fixture for PersonRotationModelParentTest.
    /// </summary>
    /// <remarks>
    /// Created by: SavaniN
    /// Created date: 2008-10-07
    /// </remarks>
    [TestFixture]
    public class PersonRotationCurrentRotationComparerTest
    {
        #region Private Members

        private int result;
        private Person person1, person2;
        private Rotation rotation1, rotation2;
        private PersonRotationModelParent _modelParent1, _modelParent2;
        private readonly PersonRotationCurrentRotationComparer<PersonRotationModelParent, IPersonRotation, IRotation> comparer =
            new PersonRotationCurrentRotationComparer<PersonRotationModelParent, IPersonRotation, IRotation>();

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
            _modelParent2.CurrentRotation = rotation2;

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
            _modelParent1.CurrentRotation = rotation1;
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
            _modelParent1.CurrentRotation = rotation1;
            _modelParent2 =
                new PersonRotationModelParent(person2, null);
            _modelParent2.CurrentRotation = rotation2;

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
            _modelParent1.CurrentRotation = rotation2;
            _modelParent2 =
                new PersonRotationModelParent(person2, null);
            _modelParent2.CurrentRotation = rotation1;

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
            _modelParent1.CurrentRotation = rotation1;
            _modelParent2 =
                new PersonRotationModelParent(person2, null);
            _modelParent2.CurrentRotation = rotation1;

            result = comparer.Compare(_modelParent1, _modelParent2);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        #endregion
    }
}
