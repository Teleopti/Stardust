using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;


namespace Teleopti.Ccc.DomainTest.Scheduling.PersonalAccount
{
    [TestFixture]
    public class PersonAccountCloserTest
    {
        private PersonAccountCloser _target;
        private IPersonAccountCollection _personAccounts;
        private DateOnly _date;
        private IAccount _thisPersonAccount;
        private IAccount _previousPersonAccount;
        private IAbsence _absence;

        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _thisPersonAccount = _mockRepository.StrictMock<IAccount>();
            _previousPersonAccount = _mockRepository.StrictMock<IAccount>();
            _absence = new Absence();
            _personAccounts = _mockRepository.StrictMock<IPersonAccountCollection>();
            _date = new DateOnly(2001, 12, 01);
            _target = new PersonAccountCloser();
            
        }

        [Test]
        public void VerifyClosePersonAccountWithNoCurrentAccountFound()
        {
            using (_mockRepository.Record())
            {
                Expect.Call(_personAccounts.Find(_absence, _date))
                    .Return(null)
                    .Repeat.AtLeastOnce();
            }
            using (_mockRepository.Playback())
            {
                bool result = _target.ClosePersonAccount(_personAccounts, _absence, _date);
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void VerifyClosePersonAccountNoPreviousAccount()
        {
            DateOnly personAccountStartDate = _date.AddDays(-2);
            TimeSpan remaining = new TimeSpan(2, 1, 1);
            TimeSpan balanceOut = new TimeSpan(2, 1, 1);

            using (_mockRepository.Record())
            {
                Expect.Call(_personAccounts.Find(_absence, _date))
                    .Return(_thisPersonAccount);
                Expect.Call(_thisPersonAccount.StartDate)
                    .Return(personAccountStartDate)
                    .Repeat.AtLeastOnce();
                Expect.Call(_personAccounts.Find(_absence, personAccountStartDate.AddDays(-1)))
                    .Return(null);
                Expect.Call(_thisPersonAccount.Remaining).Return(remaining);
                Expect.Call(_thisPersonAccount.BalanceOut).Return(balanceOut);
                Expect.Call(_thisPersonAccount.BalanceOut = balanceOut + remaining);
            }
            using (_mockRepository.Playback())
            {
                bool result = _target.ClosePersonAccount(_personAccounts, _absence, _date);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void VerifyClosePersonAccountTwoAccountsInAccountListWithPreviousAccount()
        {
            DateOnly thisPersonAccountStartDate = _date.AddDays(-2);
            TimeSpan previousAccountRemaining = new TimeSpan(2, 1, 1);
            TimeSpan previousPersonAccountBalanceOut = new TimeSpan(2, 1, 1);

            using (_mockRepository.Record())
            {
                Expect.Call(_personAccounts.Find(_absence, _date))
                    .Return(_thisPersonAccount);
                Expect.Call(_thisPersonAccount.StartDate)
                    .Return(thisPersonAccountStartDate)
                    .Repeat.AtLeastOnce();
                Expect.Call(_personAccounts.Find(_absence, thisPersonAccountStartDate.AddDays(-1)))
                    .Return(_previousPersonAccount);
                Expect.Call(_previousPersonAccount.Remaining)
                    .Return(previousAccountRemaining);
                Expect.Call(_previousPersonAccount.BalanceOut).Return(previousPersonAccountBalanceOut).Repeat.AtLeastOnce();
                Expect.Call(_previousPersonAccount.BalanceOut = previousPersonAccountBalanceOut + previousAccountRemaining);
                Expect.Call(_thisPersonAccount.BalanceIn = previousPersonAccountBalanceOut + previousAccountRemaining);
            }
            using (_mockRepository.Playback())
            {
                bool result = _target.ClosePersonAccount(_personAccounts, _absence, _date);
                Assert.IsTrue(result);
            }
        }
    }
}
