using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonAccountAssemblerTest
    {
        private PersonAccountAssembler _target;
        private PersonAccountAssembler _target2;
        private IPerson _person;
        private IScenario _scenario;
       
        private IAbsence _absence;

        [SetUp]
        public void Setup()
        {
            _target = new PersonAccountAssembler();
            _target2 = new PersonAccountAssembler();
            _person = PersonFactory.CreatePerson();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _target.Person = _person;
            _target2.Person = _person;
            _target.DefaultScenario = _scenario;
            _target2.DefaultScenario = _scenario;
            _absence = AbsenceFactory.CreateAbsence("hej");
        }

        [Test]
        public void VerifySetup()
        {
            Assert.IsNotNull(_target);
            Assert.IsNotNull(_target2);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void VerifyDoToDo()
        {
            _target.DtoToDomainEntity(null);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void VerifyDoToDo2()
        {
            _target2.DtoToDomainEntity(null);
        }

        [Test]
        public void VerifyDoToDtoTime()
        {
            var personAccount = new AccountTime(new DateOnly(2009, 1, 1));
            var timeSpan = new TimeSpan(5,2,23);
          
            var absenceAccount = new PersonAbsenceAccount(_person, _absence);
            absenceAccount.Add(personAccount);
           
            personAccount.Accrued = timeSpan;
            personAccount.Extra = timeSpan;
            personAccount.LatestCalculatedBalance = timeSpan;

            PersonAccountDto result =  _target.DomainEntityToDto(personAccount);
            Assert.IsTrue(result.IsInMinutes);
            Assert.AreEqual(result.Accrued, timeSpan.Ticks);
            Assert.AreEqual(result.BalanceIn, 0);
            Assert.AreEqual(result.Extra, timeSpan.Ticks);
            Assert.AreEqual(result.LatestCalculatedBalance, timeSpan.Ticks);
            Assert.AreEqual(result.Remaining, timeSpan.Add(timeSpan).Subtract(timeSpan).Ticks);
            Assert.AreEqual("hej", result.TrackingDescription);
        }

        [Test]
        public void VerifyDoToDtoDay()
        {
            var personAccount = new AccountDay(new DateOnly(2009, 1, 1));
            
            var absenceAccount = new PersonAbsenceAccount(_person, _absence);
            absenceAccount.Add(personAccount);
            
            personAccount.Accrued = TimeSpan.FromDays(20);
            personAccount.Extra = TimeSpan.FromDays(2);
            personAccount.LatestCalculatedBalance = TimeSpan.FromDays(6);

            PersonAccountDto result = _target2.DomainEntityToDto(personAccount);
            Assert.IsFalse(result.IsInMinutes);
            Assert.AreEqual(personAccount.Accrued.Ticks, result.Accrued);
            Assert.AreEqual(personAccount.BalanceIn.Ticks, result.BalanceIn);
            Assert.AreEqual(personAccount.Extra.Ticks, result.Extra);
            Assert.AreEqual(personAccount.LatestCalculatedBalance.Ticks, result.LatestCalculatedBalance);
            Assert.AreEqual(personAccount.Remaining.Ticks, result.Remaining);
            Assert.AreEqual(personAccount.BalanceOut.Ticks, result.BalanceOut);
            Assert.AreEqual("hej", result.TrackingDescription);
        }
    }
}
