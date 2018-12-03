using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.FakeData;

using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Comparers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Comparers
{
    /// <summary>
    /// Represents the test class for thePersonAccountDescriptionComparer.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-09
    /// </remarks>
    [TestFixture]
    public class PersonAccountDescriptionComparerTest
    {

        #region Private Members

        private int result;
        private IPerson person1, person2;
        private IPersonAccountModel _targetDay1, _targetDay2;
        private DateOnly from1, from2;
        private readonly PersonAccountDescriptionComparer comparer = new PersonAccountDescriptionComparer();
        private IAbsence _absenceDay, _absenceTime;
        private IPersonAccountCollection accounts1;
        private IPersonAccountCollection accounts2;

        #endregion

        #region Setup and Teardown Methods

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-09
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

            _absenceDay = AbsenceFactory.CreateAbsence("asd1");
            _absenceDay.Tracker = Tracker.CreateDayTracker();

            _absenceTime = AbsenceFactory.CreateAbsence("asd2");
            _absenceTime.Tracker = Tracker.CreateTimeTracker();


            accounts1 = new PersonAccountCollection(person1);
            accounts2 = new PersonAccountCollection(person2);

            // Creates the person account adopter instances
            _targetDay1 = new PersonAccountModel(null, accounts1, null, null);
            _targetDay2 = new PersonAccountModel(null, accounts2, null, null);
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Verifies the compare method with all null.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-09
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
        /// Created date: 2008-10-09
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithFirstNull()
        {
           
           
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
        /// Created date: 2008-10-09
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
        /// Created date: 2008-10-09
        /// </remarks>
        [Test]
        public void VerifyCompareMethodAscending()
        {
            // Creates the person account adopter instances
            _targetDay1 = GetPersonAccountDayWithTracker();
            _targetDay2 = GetPersonAccountTimeWithTracker();
           
            result = comparer.Compare(_targetDay1, _targetDay2);
            // Checks whether the roles are equal
            Assert.AreEqual(-1, result);
        }

        /// <summary>
        /// Verifies the compare method descending.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-09
        /// </remarks>
        [Test]
        public void VerifyCompareMethodDescending()
        {
            // Creates the person account adopter instances
            _targetDay1 = GetPersonAccountDayWithTracker();
            _targetDay2 = GetPersonAccountTimeWithTracker();
            
            result = comparer.Compare(_targetDay2, _targetDay1);

            // Checks whether the roles are equal
            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// Verifies the compare method with second with same.
        /// </summary>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-09
        /// </remarks>
        [Test]
        public void VerifyCompareMethodWithSecondWithSame()
        {
            // Creates the person account adopter instances
            _targetDay1 = GetPersonAccountTimeWithTracker();
            _targetDay2 = GetPersonAccountTimeWithTracker();

            result = comparer.Compare(_targetDay1, _targetDay2);

            // Checks whether the roles are equal
            Assert.AreEqual(0, result);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the person account day with tracker.
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-11-19
        /// </remarks>
        private IPersonAccountModel GetPersonAccountTimeWithTracker()
        {
            IAccount account = new AccountTime(from2);
            var temp = new PersonAbsenceAccount(person2, new Absence());
            temp.Absence.Tracker = Tracker.CreateTimeTracker();
            temp.Add(account);
            accounts2.Add(temp);
            return new PersonAccountModel(null, accounts2, account, null);
        }

        /// <summary>
        /// Gets the person account day with tracker.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-11-19
        /// </remarks>
        private IPersonAccountModel GetPersonAccountDayWithTracker()
        {
            IAccount account = new AccountDay(from1);
            var temp = new PersonAbsenceAccount(person1, new Absence());
            temp.Absence.Tracker = Tracker.CreateDayTracker();
            temp.Add(account);
            accounts1.Add(temp);
            return new PersonAccountModel(null, accounts1, account, null);
        }

        #endregion

    }
}
