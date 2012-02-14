using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.PersonalAccount
{
    [TestFixture]
    public class AccountTimeTest
    {
        private TimeSpan _accrued;
        private TimeSpan _extra;
        private AccountTime _target;
        private readonly DateOnly _baseDateTime = new DateOnly(2008, 4, 12);

        [SetUp]
        public void Setup()
        {
            _accrued = TimeSpan.FromMinutes(23);
            _extra = TimeSpan.FromMinutes(7);
            _target = new AccountTime(_baseDateTime);
            _target.Accrued = _accrued;
            _target.Extra = _extra;
        }

        [Test]
        public void CanCreatePersonAccount()
        {
            _target = new AccountTime(_baseDateTime);
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
            IPersonAbsenceAccount personAbsenceAccount = new PersonAbsenceAccount(PersonFactory.CreatePerson("kalle"),
                                                                                  AbsenceFactory.CreateAbsence("hej"));
            personAbsenceAccount.Add(_target);
            TimeSpan balanceIn = TimeSpan.FromHours(5);
            TimeSpan used = TimeSpan.FromDays(7);


            _target.Extra = _extra;
            _target.BalanceIn =balanceIn;
            _target.Track(used);

            Assert.IsTrue(_target.Remaining <= TimeSpan.Zero);
        }


        //[Test]
        //public void VerifyCalculateBalanceInFromEarlierPersonAccount()
        //{
        //    IAbsence absence = new Absence();
        //    TimeSpan accruedIn = TimeSpan.FromMilliseconds(45668456);
        //    var account1 = new AccountTime(_baseDateTime);
        //    account1.Accrued = accruedIn;
        //    var account2 = new AccountTime(_baseDateTime.AddDays(1));
        //    account2.BalanceIn = TimeSpan.FromMinutes(5);
        //    IPerson person = new Person();
        //    var a = new PersonAbsenceAccount(person, absence);
        //    a.Add(account2);

        //    account2.CalculateBalanceIn();
        //    Assert.AreEqual(TimeSpan.Zero, account2.BalanceIn, "Set to 0 if no erlier accounts exists");

        //    a.Add(account1);
        //    account2.CalculateBalanceIn();
        //    Assert.AreEqual(accruedIn, account2.BalanceIn, "Calculates from earlier PersonAccount BalanceOut");
        //}

        [Test]
        public void VerifyFindEarlierPersonAccount()
        {
            var paAcc = new PersonAbsenceAccount(new Person(), new Absence());

            var pAcc1 = new AccountTime(_baseDateTime);
            var pAcc2 = new AccountTime(_baseDateTime.AddDays(2));
            var pAcc3 = new AccountTime(_baseDateTime.AddDays(3));
            var pAcc4 = new AccountTime(_baseDateTime.AddDays(4));
            var pAcc5 = new AccountTime(_baseDateTime.AddDays(5));
            var pAcc6 = new AccountTime(_baseDateTime.AddDays(6));

            paAcc.Add(pAcc2);
            paAcc.Add(pAcc3);
            paAcc.Add(pAcc5);
            paAcc.Add(pAcc6);

            Assert.IsNull(pAcc2.FindEarliestPersonAccount(), "Null if first in list");
            paAcc.Add(pAcc1);
            Assert.AreEqual(pAcc1.StartDate, pAcc2.FindEarliestPersonAccount().StartDate, "Not null, gets earlier account");
            Assert.AreEqual(pAcc1.StartDate, pAcc5.FindEarliestPersonAccount().StartDate, "Gets earlier");
            paAcc.Add(pAcc4);
            Assert.AreEqual(pAcc1.StartDate, pAcc5.FindEarliestPersonAccount().StartDate, "Gets earlier in unsorted-list");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyDefaultValuesAreSet()
        {
            IPersonAbsenceAccount personAbsenceAccount = new PersonAbsenceAccount(PersonFactory.CreatePerson("kalle"),
                                                                                  AbsenceFactory.CreateAbsence("hej"));
            AccountTime pTime = new AccountTime(new DateOnly(2001, 1, 1));
            personAbsenceAccount.Add(pTime);
            Assert.AreEqual(default(TimeSpan), pTime.Accrued);
            Assert.AreEqual(default(TimeSpan), pTime.BalanceIn);
            Assert.AreEqual(default(TimeSpan), pTime.BalanceOut);
            Assert.AreEqual(default(TimeSpan), pTime.Extra);
        }

        [Test]
        public void VerifyTrackTimeSetsUsedTime()
        {
            TimeSpan trackedTime = TimeSpan.FromMinutes(12);
            _target.Track(trackedTime);
            Assert.AreEqual(12, _target.LatestCalculatedBalance.TotalMinutes);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(AccountTime)));
        }

    }
}