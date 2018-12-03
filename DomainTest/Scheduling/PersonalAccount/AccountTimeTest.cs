using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


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