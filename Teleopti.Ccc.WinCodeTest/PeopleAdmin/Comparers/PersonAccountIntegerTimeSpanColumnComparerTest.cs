#region Imports

using System;
using System.Collections.Generic;
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
    /// Represents the the test class for the PersonAccountIntegerTimeSpanColumnComparer.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-08
    /// </remarks>
    [TestFixture]
    public class PersonAccountIntegerTimeSpanColumnComparerTest
    {
        #region Private Members

        private int result;
        private IPerson person1, person2;
        private IPersonAccountModel _targetDay1, _targetDay2;
        private DateOnly from1, from2;
        private PersonAccountIntegerTimeSpanColumnComparer comparer;
        private Dictionary<IPerson, IPersonAccountCollection> _allAccounts;

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
            var ett = new PersonAccountCollection(person1);
            var two = new PersonAccountCollection(person2);

            _allAccounts = new Dictionary<IPerson, IPersonAccountCollection>
                               {
                                   {person1, ett}, 
                                   {person2, two}
                               };
            // Creates data instances
            from1 = new DateOnly(2008, 1, 3);
            from2 = new DateOnly(2009, 1, 3);
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
            // Creates the person account adopter instances
            _targetDay1 = new PersonAccountModel(null, _allAccounts[person1], null, null);
            _targetDay2 = new PersonAccountModel(null, _allAccounts[person2], null, null);

            // Instantiatest the comparer
            comparer = new PersonAccountIntegerTimeSpanColumnComparer("BalanceIn");
            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the results are equal
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
            _targetDay1 = new PersonAccountModel(null, _allAccounts[person1], null, null);
            _targetDay2 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from2, person1, 5, 3, 2);

            // Instantiatest the comparer
            comparer = new PersonAccountIntegerTimeSpanColumnComparer("BalanceIn");
            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the results are equal
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
            _targetDay2 = new PersonAccountModel(null,_allAccounts[person1],null,null);

            // Instantiatest the comparer
            comparer = new PersonAccountIntegerTimeSpanColumnComparer("BalanceIn");
            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the results are equal
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
            // Instantiatest the comparer
            comparer = new PersonAccountIntegerTimeSpanColumnComparer("BalanceIn");

            // Creates the person account adopter instances
            _targetDay1 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from1, person1, 3, 3, 2);
            _targetDay2 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountDayGridViewAdapter(from2, person2, 5, 3, 2);

            result = comparer.Compare(_targetDay1, _targetDay2);
            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);

            // Creates the person account adopter instances
            _targetDay1 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountTimeGridViewAdapter(from1, person1,
                                                                                    new TimeSpan(3, 0, 0),
                                                                                    new TimeSpan(3, 0, 0),
                                                                                    new TimeSpan(2, 0, 0));
            _targetDay2 =
                PeopleAdminUtilityTestHelper.CreatePersonAccountTimeGridViewAdapter(from2, person2, new TimeSpan(5, 0, 0),
                                                                                    new TimeSpan(3, 0, 0),
                                                                                    new TimeSpan(2, 0, 0));

            result = comparer.Compare(_targetDay1, _targetDay2);
            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);

            result = comparer.Compare(_targetDay2, _targetDay1);
            Assert.AreEqual(1, result);
        }

        #endregion
    }
}
