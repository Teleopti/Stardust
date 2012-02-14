using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class PersonAccountTimeTest
    {
        private TimeSpan _accrued;
        private TimeSpan _extra;
        private PersonAccountTimeForTest _target;
        private readonly DateOnly _baseDateTime = new DateOnly(2008, 4, 12);

        [SetUp]
        public void Setup()
        {
            _accrued = TimeSpan.FromMinutes(23);
            _extra = TimeSpan.FromMinutes(7);
            _target = new PersonAccountTimeForTest(_baseDateTime, new Absence(), _accrued, _extra);
        }

        [Test]
        public void CanCreatePersonAccount()
        {
            _target = new PersonAccountTimeForTest(_baseDateTime, new Absence());
            _target.Accrued = _accrued;
            _target.Extra = _extra;
            Assert.IsNotNull(_target);
            Assert.AreEqual(_accrued, _target.Accrued);
            Assert.AreEqual(_extra, _target.Extra);
            Assert.AreEqual(TimeSpan.Zero, _target.BalanceIn);
            Assert.AreEqual(TimeSpan.Zero, _target.LatestCalculatedBalance);

        }

        [Test]
        public void CanHandleProperties()
        {
            TimeSpan fakelatestCalculated = TimeSpan.FromMinutes(12);
            var newDateTime = _target.StartDate.AddDays(1);
            TimeSpan oldExtra = _target.Extra;
            _target.Extra = oldExtra.Add(TimeSpan.FromMinutes(5));
            _target.LatestCalculatedBalance = fakelatestCalculated;
            _target.StartDate = newDateTime;
            Assert.AreEqual(newDateTime, _target.StartDate, "Verify set/get StartDateTime");
            Assert.AreNotEqual(oldExtra, _target.Extra, "Verify set/get Extra");
            Assert.AreEqual(fakelatestCalculated, _target.LatestCalculatedBalance);
        }

        [Test]
        public void TimeLeftCanBeNegative()
        {
            TimeSpan balanceIn = TimeSpan.FromHours(5);
            TimeSpan used = TimeSpan.FromDays(7);


            _target.Extra = _extra;
            _target.SetBalanceIn(balanceIn);
            _target.Track(used);

            Assert.AreEqual(balanceIn.Subtract(used).Add(_extra).Add(_accrued), _target.BalanceOut);
            Assert.IsTrue(_target.BalanceOut <= TimeSpan.Zero);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyFactoryDependsOnAbsence()
        {
            var startDateTime = new DateOnly(2001, 1, 1);
            IAbsence absenceTime = new Absence { Tracker = Tracker.CreateTimeTracker() };
            IAbsence absenceComp = new Absence { Tracker = Tracker.CreateCompTracker() };
            Assert.IsInstanceOf<PersonAccountTime>(PersonAccount.CreatePersonAccount(startDateTime, absenceTime));
            Assert.IsInstanceOf<PersonAccountTime>(PersonAccount.CreatePersonAccount(startDateTime, absenceComp));
        }

        [Test]
        public void VerifyCalculateBalanceInFromEarlierPersonAccount()
        {
            IAbsence absence = new Absence();
            TimeSpan accruedIn = TimeSpan.FromMilliseconds(45668456);
            PersonAccountTimeForTest personAccount1 = new PersonAccountTimeForTest(_baseDateTime, absence, accruedIn, TimeSpan.Zero);
            PersonAccountTimeForTest personAccount2 = new PersonAccountTimeForTest(_baseDateTime.AddDays(1), absence, TimeSpan.Zero, TimeSpan.Zero);
            personAccount2.SetBalanceIn(TimeSpan.FromMinutes(5));
            IPerson person = new Person();
            person.AddPersonAccount(personAccount2);

            personAccount2.CalculateBalanceIn();
            Assert.AreEqual(TimeSpan.Zero, personAccount2.BalanceIn, "Set to 0 if no erlier accounts exists");

            person.AddPersonAccount(personAccount1);
            personAccount2.CalculateBalanceIn();
            Assert.AreEqual(accruedIn, personAccount2.BalanceIn, "Calculates from earlier PersonAccount BalanceOut");
        }

        [Test]
        public void VerifyFindEarlierPersonAccount()
        {
            IAbsence absence1 = new Absence { Tracker = Tracker.CreateDayTracker() };

            PersonAccountTimeForTest pAcc1 = new PersonAccountTimeForTest(_baseDateTime, absence1 );
            PersonAccountTimeForTest pAcc2 = new PersonAccountTimeForTest(_baseDateTime.AddDays(2), absence1);
            PersonAccountTimeForTest pAcc3 = new PersonAccountTimeForTest(_baseDateTime.AddDays(3), absence1);
            PersonAccountTimeForTest pAcc4 = new PersonAccountTimeForTest(_baseDateTime.AddDays(4), absence1 );
            PersonAccountTimeForTest pAcc5 = new PersonAccountTimeForTest(_baseDateTime.AddDays(5), absence1 );
            PersonAccountTimeForTest pAcc6 = new PersonAccountTimeForTest(_baseDateTime.AddDays(6), absence1 );

            IList<IPersonAccount> pAccList = new List<IPersonAccount> { pAcc2, pAcc3, pAcc5, pAcc6 };

            Assert.IsNull(pAcc2.FindEarlierPersonAccount(pAccList), "Null if first in list");
            pAccList.Add(pAcc1);
            Assert.AreEqual(pAcc1.StartDate, pAcc2.FindEarlierPersonAccount(pAccList).StartDate, "Not null, gets earlier account");
            Assert.AreEqual(pAcc3.StartDate, pAcc5.FindEarlierPersonAccount(pAccList).StartDate, "Gets earlier");
            pAccList.Add(pAcc4);
            Assert.AreEqual(pAcc4.StartDate, pAcc5.FindEarlierPersonAccount(pAccList).StartDate, "Gets earlier in unsorted-list");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyDefaultValuesAreSet()
        {
            IAbsence abs = new Absence { Tracker = Tracker.CreateTimeTracker() };
            PersonAccountTime pTime = new PersonAccountTime(new DateOnly(2001, 1, 1), abs);
            Assert.AreEqual(default(TimeSpan), pTime.Accrued);
            Assert.AreEqual(default(TimeSpan), pTime.BalanceIn);
            Assert.AreEqual(default(TimeSpan), pTime.BalanceOut);
            Assert.AreEqual(default(TimeSpan), pTime.Extra);
        }



        [Test]
        public void VerifyFindEarlierPersonAccountOnlyCountsSameAbsence()
        {

            IAbsence absence1 = new Absence { Tracker = Tracker.CreateDayTracker() };
            IAbsence absence2 = new Absence { Tracker = Tracker.CreateDayTracker() };
            PersonAccountTimeForTest pAcc1 = new PersonAccountTimeForTest(_baseDateTime, absence1 );
            PersonAccountTimeForTest pAcc2 = new PersonAccountTimeForTest(_baseDateTime.AddDays(2), absence2);
            PersonAccountTimeForTest pAcc3 = new PersonAccountTimeForTest(_baseDateTime.AddDays(3), absence1);

            IList<IPersonAccount> pAccList = new List<IPersonAccount> { pAcc1, pAcc2, pAcc3 };

            Assert.AreEqual(pAcc1.StartDate, pAcc3.FindEarlierPersonAccount(pAccList).StartDate);

        }

        [Test]
        public void VerifyTrackTimeSetsUsedTime()
        {
            TimeSpan trackedTime = TimeSpan.FromMinutes(12);
            _target.Track(trackedTime);
            Assert.AreEqual(12, _target.LatestCalculatedBalanceForTest.TotalMinutes);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(PersonAccountTime)));
        }

        #region helpclasses




        private class PersonAccountTimeForTest : PersonAccountTime
        {
            public PersonAccountTimeForTest(DateOnly startDateTime, IAbsence absence)
                : base(startDateTime, absence)
            {

            }

            public PersonAccountTimeForTest(DateOnly startDateTime, IAbsence absence, TimeSpan accrued, TimeSpan extra)
                : base(startDateTime, absence)
            {
                Accrued = accrued;
                Extra = extra;
            }


            public void SetBalanceIn(TimeSpan newValue)
            {
                BalanceIn = newValue;
            }

            public TimeSpan LatestCalculatedBalanceForTest
            {
                get { return LatestCalculatedBalance; }
            }

            public IPersonAccount FindEarlierPersonAccount(IList<IPersonAccount> personAccounts)
            {
                return FindEarliestPersonAccount(personAccounts);
            }

        }
        #endregion //helpclasses
    }
}
