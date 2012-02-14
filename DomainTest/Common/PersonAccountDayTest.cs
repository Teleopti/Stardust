using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.DomainTest.Helper;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PersonAccountDayTest
    {
        private int _extra;
        private int _accrued;
        private PersonAccountDayForTest _target;
        private Description _trackingDescription;
        private readonly DateOnly _baseDateTime = new DateOnly(2008, 4, 12);
        private IAbsence _absenceWithdayTracker;

        [SetUp]
        public void Setup()
        {
            _accrued = 23;
            _extra = 7;
            
            _absenceWithdayTracker = new Absence {Tracker = Tracker.CreateDayTracker()};
            _trackingDescription= new Description("For test");
            _target = new PersonAccountDayForTest(_baseDateTime, _absenceWithdayTracker, _accrued, _extra);
            _target.TrackingDescription = _trackingDescription;
        }

        public void VerifyFactoryDependsOnAbsence()
        {

            Assert.IsInstanceOf<PersonAccountDay>(PersonAccount.CreatePersonAccount(new DateOnly(2001, 1, 1), _absenceWithdayTracker));
        }

        public void VerifyCreatesPersonAccountDayBasedOnTracker()
        {
            IPersonAccount p = PersonAccount.CreatePersonAccount(new DateOnly(2001, 1, 1), _absenceWithdayTracker);
            Assert.AreEqual(_absenceWithdayTracker, p.TrackingAbsence);
            Assert.IsInstanceOf<PersonAccountDay>(p);
        }

        [Test]
        public void CanCreatePersonAccount()
        {
            _target = new PersonAccountDayForTest(_baseDateTime, new Absence(), _accrued);
            _target.Extra = TimeSpan.FromDays(_extra);
            Assert.IsNotNull(_target);
            Assert.AreEqual(TimeSpan.FromDays(_accrued), _target.Accrued);
            Assert.AreEqual(TimeSpan.FromDays(_extra), _target.Extra);
            Assert.AreEqual(TimeSpan.Zero, _target.BalanceIn);
            Assert.AreEqual(TimeSpan.Zero,_target.LatestCalculatedBalance);
        }

        [Test]
        public void CanHandleProperties()
        {
            var newDateTime = _target.StartDate.AddDays(1);
            var oldExtra = _target.Extra;
            _target.Extra = oldExtra + TimeSpan.FromDays(2);
            _target.StartDate = newDateTime;
            _target.TrackingAbsence = new Absence {Description = new Description("for test")};
            _target.LatestCalculatedBalance = TimeSpan.FromDays(5);
            Assert.AreEqual(newDateTime, _target.StartDate, "Verify set/get StartDateTime");
            Assert.AreNotEqual(oldExtra, _target.Extra, "Verify set/get Extra");
            Assert.AreEqual(TimeSpan.FromDays(5), _target.LatestCalculatedBalance);
        }

        [Test]
        public void CanGetBalanceOut()
        {
            var used = 12;
            var balanceIn = 30;
            _target.Track(TimeSpan.FromDays(used));
            _target.SetBalanceIn(TimeSpan.FromDays(balanceIn));
           
            Assert.AreEqual(TimeSpan.FromDays(balanceIn + _accrued + _extra - used), _target.BalanceOut);
        }

        [Test]
        public void BalanceOutCanBeNegative()
        {
            var balanceIn = 5;
            var used = 44;

            _target.Extra = TimeSpan.FromDays(_extra);
            _target.SetBalanceIn(TimeSpan.FromDays(balanceIn));
            _target.Track(TimeSpan.FromDays(used));

            Assert.AreEqual(TimeSpan.FromDays(_accrued + balanceIn + _extra - used), _target.BalanceOut);
            Assert.IsTrue(_target.BalanceOut <= TimeSpan.Zero);
        }

        [Test]
        public void VerifyCalculateBalanceInFromEarlierPersonAccount()
        {
            
            var personAccount1 = new PersonAccountDayForTest(_baseDateTime, _absenceWithdayTracker, 10 , 7);
            var personAccount2 = new PersonAccountDayForTest(_baseDateTime.AddDays(1), _absenceWithdayTracker, 0, 0);
            personAccount2.SetBalanceIn(TimeSpan.FromDays(5));
            IPerson person = new Person();
            person.AddPersonAccount(personAccount2);

            personAccount2.CalculateBalanceIn();
            Assert.AreEqual(TimeSpan.Zero, personAccount2.BalanceIn, "Set to 0 if no erlier accounts exists");

            person.AddPersonAccount(personAccount1);

            personAccount2.CalculateBalanceIn();
            Assert.AreEqual(TimeSpan.FromDays(17), personAccount2.BalanceIn, "Calculates from earlier PersonAccount BalanceOut");
        }

        [Test]
        public void VerifyCalculateBalanceInUpdatesNextPersonAccountBalanceIn()
        {
            PersonAccountDayForTest personAccount1 = new PersonAccountDayForTest(_baseDateTime, _absenceWithdayTracker, 10, 7);
            PersonAccountDayForTest personAccount2 = new PersonAccountDayForTest(_baseDateTime.AddDays(1), _absenceWithdayTracker, 0, 0);
            personAccount2.SetBalanceIn(TimeSpan.FromDays(5));
            IPerson person = new Person();
            person.AddPersonAccount(personAccount2);

            personAccount2.CalculateBalanceIn();
            Assert.AreEqual(TimeSpan.Zero, personAccount2.BalanceIn, "Set to 0 if no erlier accounts exists");

            person.AddPersonAccount(personAccount1);

            personAccount1.CalculateBalanceIn();
            Assert.AreEqual(TimeSpan.FromDays(17), personAccount2.BalanceIn, "Calculates from earlier PersonAccount BalanceOut");
        }

        [Test]
        public void VerifyFindEarlierPersonAccount()
        {

            var pAcc1 = new PersonAccountDayForTest(_baseDateTime, _absenceWithdayTracker, 0);
            var pAcc2 = new PersonAccountDayForTest(_baseDateTime.AddDays(2), _absenceWithdayTracker, 0);
            var pAcc3 = new PersonAccountDayForTest(_baseDateTime.AddDays(3), _absenceWithdayTracker, 0);
            var pAcc4 = new PersonAccountDayForTest(_baseDateTime.AddDays(4), _absenceWithdayTracker, 0);
            var pAcc5 = new PersonAccountDayForTest(_baseDateTime.AddDays(5), _absenceWithdayTracker, 0);
            var pAcc6 = new PersonAccountDayForTest(_baseDateTime.AddDays(6), _absenceWithdayTracker, 0);

            IList<IPersonAccount> pAccList = new List<IPersonAccount> { pAcc2, pAcc3, pAcc5, pAcc6 };

            Assert.IsNull(pAcc2.FindEarlierPersonAccount(pAccList), "Null if first in list");
            pAccList.Add(pAcc1);
            Assert.AreEqual(pAcc1.StartDate, pAcc2.FindEarlierPersonAccount(pAccList).StartDate, "Not null, gets earlier account");
            Assert.AreEqual(pAcc3.StartDate, pAcc5.FindEarlierPersonAccount(pAccList).StartDate, "Gets earlier");
            pAccList.Add(pAcc4);
            Assert.AreEqual(pAcc4.StartDate, pAcc5.FindEarlierPersonAccount(pAccList).StartDate, "Gets earlier in unsorted-list");
        }

       

        [Test]
        public void VerifyFindEarlierPersonAccountOnlyCountsSameAbsence()
        {
            IAbsence absence1 = new Absence {Tracker = Tracker.CreateDayTracker()};
            IAbsence absence2 = new Absence {Tracker = Tracker.CreateDayTracker()};
            var pAcc1 = new PersonAccountDayForTest(_baseDateTime.AddDays(1),absence1,0);
            var pAcc2 = new PersonAccountDayForTest(_baseDateTime.AddDays(2), absence2,0);
            var pAcc3 = new PersonAccountDayForTest(_baseDateTime.AddDays(3), absence1, 0);

            IList<IPersonAccount> pAccList = new List<IPersonAccount> { pAcc1, pAcc2, pAcc3 };

            Assert.AreEqual(pAcc1.StartDate, pAcc3.FindEarlierPersonAccount(pAccList).StartDate);

        }

        [Test]
        public void VerifyCloneWorks()
        {
            PersonAccountDayForTest clonedEntity = (PersonAccountDayForTest)_target.Clone();
            Assert.AreNotEqual(clonedEntity, _target);
            Assert.AreNotSame(clonedEntity, _target);
        }

        [Test]
        public void VerifyUsedOccasionsIsSet()
        {
            _target.Track(TimeSpan.FromDays(12));
            Assert.AreEqual(TimeSpan.FromDays(12), _target.TestLatestCalculatedBalance);
            //TrackTime does nothing for now
            _target.Track(TimeSpan.FromMinutes(12));
        }

        
        [Test]
        public void VerifyPersonAccountFactoryCreatesPersonAccountDayIfAbsenceHasNoTracker()
        {
            //It should be possible to create a PersonAccount without a Tracker, the Null-check must be within the Account
            //Maybe change so Absence has a default Tracker. For now, we create a PersonAccountDay
            IAbsence absenceWithoutTracker = new Absence();
            IPersonAccount defaultPersonAccount = PersonAccount.CreatePersonAccount(_baseDateTime, absenceWithoutTracker);
            Assert.IsInstanceOf<PersonAccountDay>(defaultPersonAccount);
            
        }


       
        public void VerifyDefaultValuesAreSet()
        {

            PersonAccountDay pday = new PersonAccountDay(new DateOnly(2001, 1, 1), _absenceWithdayTracker);
            Assert.AreEqual(default(TimeSpan), pday.Accrued);
            Assert.AreEqual(default(TimeSpan), pday.BalanceIn);
            Assert.AreEqual(default(TimeSpan), pday.BalanceOut);
            Assert.AreEqual(default(TimeSpan), pday.Extra);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(PersonAccountDay)));
        }

        [Test]
        public void VerifyGetsEndDateTimeFromNextPersonAccountWithSameAbsence()
        {
            var dateTime1 = new DateOnly(2001,1,1);
            var dateTime2 = new DateOnly(2001,1,2);
            var dateTime3 = new DateOnly(2001,1,3);
            var dateTime4 = new DateOnly(2001,1,4);
            IAbsence anotherAbsence = new Absence {Tracker = Tracker.CreateDayTracker()};
            IPerson person = new Person();
            IPersonAccount pAcc1 = new PersonAccountDay(dateTime1, _absenceWithdayTracker);
            IPersonAccount pAcc2 = new PersonAccountDay(dateTime2, _absenceWithdayTracker);
            IPersonAccount pAcc3 = new PersonAccountDay(dateTime3, anotherAbsence);
            IPersonAccount pAcc4 = new PersonAccountDay(dateTime4, _absenceWithdayTracker);
            person.AddPersonAccount(pAcc1);
            person.AddPersonAccount(pAcc2);
            person.AddPersonAccount(pAcc3);
            person.AddPersonAccount(pAcc4);
           
            Assert.AreEqual(dateTime2.AddDays(-1),pAcc1.Period().EndDate);
            Assert.AreEqual(dateTime4.AddDays(-1),pAcc2.Period().EndDate);
            Assert.IsTrue(pAcc3.StartDate.AddDays(3000) < pAcc3.Period().EndDate, "Check that the period is big enough, ~8 years is reasonable");
            Assert.IsTrue(DateTime.MaxValue.ToUniversalTime().Subtract(TimeSpan.FromDays(100)) > pAcc3.Period().EndDate,"Because projection is adding time it must be atleast 100 days margin ");
        }

        #region helpclasses
        
        private class PersonAccountDayForTest : PersonAccountDay
        {
            public PersonAccountDayForTest(DateOnly startDateTime, IAbsence absence, int accrued)
                : base(startDateTime, absence)
            {
                Accrued = TimeSpan.FromDays(accrued);
            }

            public PersonAccountDayForTest(DateOnly startDateTime, IAbsence absence, int accrued, int extra)
                : base(startDateTime, absence)
            {
                Accrued = TimeSpan.FromDays(accrued);
                Extra = TimeSpan.FromDays(extra);
            }

            public TimeSpan TestLatestCalculatedBalance
            {
                get { return LatestCalculatedBalance; }
            }


            public void SetBalanceIn(TimeSpan newValue)
            {
                BalanceIn = newValue;
            }

            public IPersonAccount FindEarlierPersonAccount(IList<IPersonAccount> personAccounts)
            {
                return FindEarliestPersonAccount(personAccounts);
            }

        }
        #endregion //helpclasses
    }

    
}