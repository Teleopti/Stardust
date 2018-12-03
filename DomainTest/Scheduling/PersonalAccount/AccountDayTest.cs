using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.PersonalAccount
{
    [TestFixture]
    public class AccountDayTest
    {
        private int _extra;
        private int _accrued;
        private AccountDayForTest _target;
        private readonly DateOnly _startDate = new DateOnly(2008, 4, 12);
        private MockRepository _mocks;
        private IPersonAbsenceAccount _parent;

        [SetUp]
        public void Setup()
        {
            _accrued = 23;
            _extra = 7;

            _mocks = new MockRepository();
            _parent = _mocks.StrictMock<IPersonAbsenceAccount>();
            _target = new AccountDayForTest(_startDate, _accrued, _extra, _parent);
        }

        [Test]
        public void ShouldNotThrowWhen2PeriodsWithSameStartDate()
        {
            var newerAccountWithSameDate = _mocks.StrictMock<IAccount>();
            var accounts = new[] {newerAccountWithSameDate, _target};

            _mocks.Record();

            Expect.Call(_parent.AccountCollection()).Return(accounts);
            Expect.Call(newerAccountWithSameDate.StartDate).Return(_startDate);
			Expect.Call(_parent.Person).Return(PersonFactory.CreatePerson());

            _mocks.ReplayAll();

            var period = _target.Period();

            Assert.That(period.StartDate, Is.EqualTo(_startDate));
            Assert.That(period.EndDate, Is.EqualTo(_startDate));
        }

        [Test]
        public void CanCreatePersonAccount()
        {
            _target = new AccountDayForTest(_startDate, _accrued);
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
           // _target.Absence = new Absence {Description = new Description("for test")};
            _target.LatestCalculatedBalance = TimeSpan.FromDays(5);
            Assert.AreEqual(newDateTime, _target.StartDate, "Verify set/get StartDateTime");
            Assert.AreNotEqual(oldExtra, _target.Extra, "Verify set/get Extra");
            Assert.AreEqual(TimeSpan.FromDays(5), _target.LatestCalculatedBalance);
        }

        [Test]
        public void CanGetBalanceOut()
        {
            IPersonAbsenceAccount personAbsenceAccount = new PersonAbsenceAccount(PersonFactory.CreatePerson("kalle"),
                                                                  AbsenceFactory.CreateAbsence("hej"));
            personAbsenceAccount.Add(_target);
            var used = 12;
            var balanceIn = 30;
            _target.Track(TimeSpan.FromDays(used));
            _target.SetBalanceIn(TimeSpan.FromDays(balanceIn));
           
        }

        [Test]
        public void RemainingCanBeNegative()
        {
            IPersonAbsenceAccount personAbsenceAccount = new PersonAbsenceAccount(PersonFactory.CreatePerson("kalle"),
                                                                              AbsenceFactory.CreateAbsence("hej"));
            personAbsenceAccount.Add(_target);
            var balanceIn = 5;
            var used = 44;

            _target.Extra = TimeSpan.FromDays(_extra);
            _target.SetBalanceIn(TimeSpan.FromDays(balanceIn));
            _target.Track(TimeSpan.FromDays(used));

            Assert.IsTrue(_target.Remaining <= TimeSpan.Zero);
        }

        [Test]
        public void VerifyCloneWorks()
        {
            var clonedEntity = _target.Clone();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(typeof(AccountDay)));
        }

        #region helpclasses
        
        private class AccountDayForTest : AccountDay
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
            public AccountDayForTest(DateOnly startDateTime, int accrued)
                : base(startDateTime)
            {
                Accrued = TimeSpan.FromDays(accrued);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
            public AccountDayForTest(DateOnly startDateTime, int accrued, int extra, IPersonAbsenceAccount parent)
                : base(startDateTime)
            {
                Accrued = TimeSpan.FromDays(accrued);
                Extra = TimeSpan.FromDays(extra);
                SetParent(parent);
            }

            public TimeSpan TestLatestCalculatedBalance
            {
                get { return LatestCalculatedBalance; }
            }


            public void SetBalanceIn(TimeSpan newValue)
            {
                BalanceIn = newValue;
            }

        }
        #endregion //helpclasses
    }
}