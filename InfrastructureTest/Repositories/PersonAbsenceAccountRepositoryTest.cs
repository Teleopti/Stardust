using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;

using System;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class PersonAbsenceAccountRepositoryTest : RepositoryTest<IPersonAbsenceAccount>
    {
		private IAbsence absence;
		private IPerson person;

		protected override void ConcreteSetup()
		{
			person = createPersonInDb();
			absence = createAbsenceInDb();
		}

		protected override IPersonAbsenceAccount CreateAggregateWithCorrectBusinessUnit()
        {
            var paAcc = new PersonAbsenceAccount(person, absence);
            paAcc.Add(new AccountTime(new DateOnly(2000,1,1)));
            paAcc.Add(new AccountDay(new DateOnly(1900,1,1)));
            return paAcc;
        }

        private Absence createAbsenceInDb()
        {
            var absence = new Absence {Description = new Description("sdf")};
            PersistAndRemoveFromUnitOfWork(absence);
            return absence;
        }

        private Person createPersonInDb()
        {
            var person=new Person();
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
            PersistAndRemoveFromUnitOfWork(person);
            return person;
        }

        protected override void VerifyAggregateGraphProperties(IPersonAbsenceAccount loadedAggregateFromDatabase)
        {
            var compareWith = CreateAggregateWithCorrectBusinessUnit();
			compareWith.Person.Should().Be.EqualTo(person);
			compareWith.Absence.Should().Be.EqualTo(absence);
            Assert.AreEqual(compareWith.AccountCollection().Count(), loadedAggregateFromDatabase.AccountCollection().Count());
            foreach (var account in compareWith.AccountCollection())
            {
                loadedAggregateFromDatabase.AccountCollection().Contains(account);
            }
        }

        protected override Repository<IPersonAbsenceAccount> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return PersonAbsenceAccountRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

        [Test]
        public void VerifyLoadAllAccounts()
        {
            var person2 = createPersonInDb();
            var paAcc1 = new PersonAbsenceAccount(person, absence);
            var paAcc2 = new PersonAbsenceAccount(person2, absence);
            paAcc1.Add(new AccountTime(new DateOnly(2000,1,1)));
            paAcc2.Add(new AccountTime(new DateOnly(2000,1,1)));
            paAcc2.Add(new AccountTime(new DateOnly(2001,1,1)));
			
            PersistAndRemoveFromUnitOfWork(paAcc1);
            PersistAndRemoveFromUnitOfWork(paAcc2);

            var rep = PersonAbsenceAccountRepository.DONT_USE_CTOR(UnitOfWork);
            var result = rep.LoadAllAccounts();
            Assert.AreEqual(1, result[person].Count());
            Assert.AreEqual(1, result[person2].Count());
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[person].Find(absence).AccountCollection()));
        }

        [Test]
        public void ShouldFindByUsers()
        {
            var person2 = createPersonInDb();
            var paAcc1 = new PersonAbsenceAccount(person, absence);
            var paAcc2 = new PersonAbsenceAccount(person2, absence);
            paAcc1.Add(new AccountTime(new DateOnly(2000, 1, 1)));
            paAcc2.Add(new AccountTime(new DateOnly(2000, 1, 1)));
            paAcc2.Add(new AccountTime(new DateOnly(2001, 1, 1)));


            PersistAndRemoveFromUnitOfWork(paAcc1);
            PersistAndRemoveFromUnitOfWork(paAcc2);

            var rep = PersonAbsenceAccountRepository.DONT_USE_CTOR(UnitOfWork);
            IList<IPerson> persons = new List<IPerson> {person};
            var result = rep.FindByUsers(persons);
            Assert.AreEqual(1, result[person].Count());
            Assert.AreEqual(false, result.ContainsKey(person2));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[person].Find(absence).AccountCollection()));    
        }

		[Test]
		public void ShouldFindSelectedUserWithAccount()
		{
			var period = new DateOnlyPeriod(new DateOnly(2018, 10, 1), new DateOnly(2018, 10, 20));
			var otherPerson = createPersonInDb();
			var personToFindAbsenceAccount = new PersonAbsenceAccount(person, absence);
			var otherPersonAbsenceAccount = new PersonAbsenceAccount(otherPerson, absence);
			var accountToFind = new AccountTime(period.StartDate);
			var otherPersonAccount = new AccountTime(period.StartDate);
			personToFindAbsenceAccount.Add(accountToFind);
			otherPersonAbsenceAccount.Add(otherPersonAccount);
			PersistAndRemoveFromUnitOfWork(personToFindAbsenceAccount);
			PersistAndRemoveFromUnitOfWork(otherPersonAbsenceAccount);
			var repository = PersonAbsenceAccountRepository.DONT_USE_CTOR(UnitOfWork);

			var result = repository.LoadByUsers(new[] {person});

			result.Count.Should().Be.EqualTo(1);
			result[person].PersonAbsenceAccounts().Single().AccountCollection().Single().Should().Be.EqualTo(accountToFind);
		}

		[Test]
		public void ShouldIncludeAccountCollection()
		{
			var period = new DateOnlyPeriod(new DateOnly(2018, 10, 1), new DateOnly(2018, 10, 20));
			var personToFindAbsenceAccount = new PersonAbsenceAccount(person, absence);
			var accountToFind = new AccountTime(period.StartDate.AddDays(-10));
			var otherAccount = new AccountTime(period.StartDate.AddDays(-50));
			personToFindAbsenceAccount.Add(accountToFind);
			personToFindAbsenceAccount.Add(otherAccount);
			PersistAndRemoveFromUnitOfWork(personToFindAbsenceAccount);
			var repository = PersonAbsenceAccountRepository.DONT_USE_CTOR(UnitOfWork);

			var result = repository.LoadByUsers(new[] { person });

			Session.Close();
			result[person].PersonAbsenceAccounts().Single().AccountCollection().Any().Should().Be.True();
		}

        [Test]
        public void NoDuplicatePersonAbsences()
        {
            var one = new PersonAbsenceAccount(person, absence);
            var two = new PersonAbsenceAccount(person, absence);
            try
            {
                CleanUpAfterTest();
                var rep = PersonAbsenceAccountRepository.DONT_USE_CTOR(UnitOfWork);
                rep.Add(one);
                rep.Add(two);
                Assert.Throws<ConstraintViolationException>(()=>UnitOfWork.PersistAll());
            }
            finally
            {
                UnitOfWork.Dispose();
                using(var nySess = Session.SessionFactory.OpenSession())
                {
                    nySess.Delete("from PersonAbsenceAccount");
                    nySess.Flush();
                }
            }
        }

        [Test]
        public void VerifyLoadAllAccountsReturnGenerousDictionary()
        {
            var rep = PersonAbsenceAccountRepository.DONT_USE_CTOR(UnitOfWork);
            var dic = rep.LoadAllAccounts();
            var p = new Person();
            Assert.IsNotNull(dic[p]);
        }
		
		[Test]
		public void VerifyCanFindAccountsForOnePersonWithAbsence()
		{
			var account = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(account);

			var rep = PersonAbsenceAccountRepository.DONT_USE_CTOR(UnitOfWork);
			var foundAccount = rep.Find(person, absence);

			Assert.IsNotNull(foundAccount);
			Assert.AreEqual(account.AccountCollection().Count(), foundAccount.AllPersonAccounts().Count());
		}

		[Test]
        public void VerifyCanFindAccountsForOnePerson()
        {
            var account = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(account);

            var rep = PersonAbsenceAccountRepository.DONT_USE_CTOR(UnitOfWork);
            var foundAccount = rep.Find(person);
            
            Assert.IsNotNull(foundAccount);
            Assert.AreEqual(account.AccountCollection().Count(),foundAccount.AllPersonAccounts().Count());
        }
    }
}