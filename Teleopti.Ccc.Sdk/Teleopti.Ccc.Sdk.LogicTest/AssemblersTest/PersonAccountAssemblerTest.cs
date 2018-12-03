using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonAccountAssemblerTest
    {
        [Test]
        public void VerifyDoToDo()
        {
			var target = new PersonAccountAssembler();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			target.Person = person;
			target.DefaultScenario = scenario;
			Assert.Throws<NotImplementedException>(() => target.DtoToDomainEntity(new PersonAccountDto()));
        }
		
        [Test]
        public void VerifyDoToDtoTime()
		{
			var target = new PersonAccountAssembler();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			target.Person = person;
			target.DefaultScenario = scenario;
			var absence = AbsenceFactory.CreateAbsence("hej");

			var personAccount = new AccountTime(new DateOnly(2009, 1, 1));
            var timeSpan = new TimeSpan(5,2,23);
          
            var absenceAccount = new PersonAbsenceAccount(person, absence);
            absenceAccount.Add(personAccount);
           
            personAccount.Accrued = timeSpan;
            personAccount.Extra = timeSpan;
            personAccount.LatestCalculatedBalance = timeSpan;

            PersonAccountDto result =  target.DomainEntityToDto(personAccount);
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
			var target = new PersonAccountAssembler();
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			target.Person = person;
			target.DefaultScenario = scenario;

			var absence = AbsenceFactory.CreateAbsence("hej");
			var personAccount = new AccountDay(new DateOnly(2009, 1, 1));
            
            var absenceAccount = new PersonAbsenceAccount(person, absence);
            absenceAccount.Add(personAccount);
            
            personAccount.Accrued = TimeSpan.FromDays(20);
            personAccount.Extra = TimeSpan.FromDays(2);
            personAccount.LatestCalculatedBalance = TimeSpan.FromDays(6);

            PersonAccountDto result = target.DomainEntityToDto(personAccount);
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
