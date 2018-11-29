#region Imports

using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.FakeData;


#endregion

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Represents the test class for the PersonAccountDateComparer.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-08
    /// </remarks>
    [TestFixture]
    public class PersonAccountDateComparerTest
    {
        #region Private Members

        private int result;
        private IPerson person1, person2;
        private IPersonAccountModel _targetDay1, _targetDay2;
        private DateOnly from1, from2;
        private readonly PersonAccountDateComparer comparer = new PersonAccountDateComparer();

        #endregion

        #region Setup and Teardown Methods

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            // Creates person instances
            person1 = PersonFactory.CreatePerson("Test Person A");
            person2 = PersonFactory.CreatePerson("Test Person B");

            // Creates data instances
            from1 = new DateOnly(2008, 1, 3);
            from2 = new DateOnly(2009, 1, 3);

            _targetDay1 = new PersonAccountModel(null, new PersonAccountCollection(person1), null, null);
            _targetDay2 = new PersonAccountModel(null, new PersonAccountCollection(person2), null, null);


           
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Verifies the compare method with all null.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithAllNull()
        {
            
       
            // Compares the values
            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        /// <summary>
        /// Verifies the compare method with first null.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithFirstNull()
        {
            // Creates the person account adopter instances
          
            _targetDay2 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from2, person1, 5, 3, 2);

            // Compares the values
            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method with second null.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithSecondNull()
        {
            // Creates the person account adopter instances
            _targetDay1 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from2, person1, 5, 3, 2);
           

            // Compares the values
            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Verifies the compare method ascending.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        [Test]
        public void VerifyCompareMethodAscending()
        {
            // Creates the person account adopter instances
            _targetDay1 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from1, person1, 3, 3, 2);
            _targetDay2 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from2, person2, 5, 3, 2);

            result = comparer.Compare(_targetDay1, _targetDay2);
            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        [Test]
        public void VerifyCompareMethodDescending()
        {
            // Creates the person account adopter instances
            _targetDay1 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from2, person1, 3, 3, 2);
            _targetDay2 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from1, person2, 5, 3, 2);

            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        [Test]
        public void VerifyCompareMethodWithSecondWithSame()
        {
            // Creates the person account adopter instances
            _targetDay1 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from2, person1, 3, 3, 2);
            _targetDay2 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from2, person2, 5, 3, 2);

            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        #endregion
    }
}