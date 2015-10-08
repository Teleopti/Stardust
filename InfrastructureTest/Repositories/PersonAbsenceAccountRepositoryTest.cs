using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class PersonAbsenceAccountRepositoryTest : RepositoryTest<IPersonAbsenceAccount>
    {

        protected override IPersonAbsenceAccount CreateAggregateWithCorrectBusinessUnit()
        {
            var person = createPersonInDb();

            var absence = createAbsenceInDb();

            var paAcc = new PersonAbsenceAccount(person, absence);
            paAcc.Add(new AccountTime(new DateOnly(2000,1,1)));
            paAcc.Add(new AccountDay(new DateOnly(1900,1,1)));
            return paAcc;
        }

        private Absence createAbsenceInDb()
        {
            var rep = new AbsenceRepository(UnitOfWork);
            var absence = new Absence {Description = new Description("sdf")};
            rep.Add(absence);
            return absence;
        }

        private Person createPersonInDb()
        {
            var rep = new PersonRepository(new ThisUnitOfWork(UnitOfWork));
            var person=new Person();
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
            rep.Add(person);
            return person;
        }

        protected override void VerifyAggregateGraphProperties(IPersonAbsenceAccount loadedAggregateFromDatabase)
        {
            var compareWith = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(compareWith.AccountCollection().Count(), loadedAggregateFromDatabase.AccountCollection().Count());
            foreach (var account in compareWith.AccountCollection())
            {
                loadedAggregateFromDatabase.AccountCollection().Contains(account);
            }
        }

        protected override Repository<IPersonAbsenceAccount> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new PersonAbsenceAccountRepository(currentUnitOfWork);
        }

        [Test]
        public void VerifyLoadAllAccounts()
        {
            var person1 = createPersonInDb();
            var person2 = createPersonInDb();
            var abs = createAbsenceInDb();
            var paAcc1 = new PersonAbsenceAccount(person1, abs);
            var paAcc2 = new PersonAbsenceAccount(person2, abs);
            paAcc1.Add(new AccountTime(new DateOnly(2000,1,1)));
            paAcc2.Add(new AccountTime(new DateOnly(2000,1,1)));
            paAcc2.Add(new AccountTime(new DateOnly(2001,1,1)));


            PersistAndRemoveFromUnitOfWork(paAcc1);
            PersistAndRemoveFromUnitOfWork(paAcc2);

            var rep = new PersonAbsenceAccountRepository(UnitOfWork);
            var result = rep.LoadAllAccounts();
            Assert.AreEqual(1, result[person1].Count());
            Assert.AreEqual(1, result[person2].Count());
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[person1].Find(abs).AccountCollection()));
        }

        [Test]
        public void ShouldFindByUsers()
        {
            var person1 = createPersonInDb();
            var person2 = createPersonInDb();
            var abs = createAbsenceInDb();
            var paAcc1 = new PersonAbsenceAccount(person1, abs);
            var paAcc2 = new PersonAbsenceAccount(person2, abs);
            paAcc1.Add(new AccountTime(new DateOnly(2000, 1, 1)));
            paAcc2.Add(new AccountTime(new DateOnly(2000, 1, 1)));
            paAcc2.Add(new AccountTime(new DateOnly(2001, 1, 1)));


            PersistAndRemoveFromUnitOfWork(paAcc1);
            PersistAndRemoveFromUnitOfWork(paAcc2);

            var rep = new PersonAbsenceAccountRepository(UnitOfWork);
            IList<IPerson> persons = new List<IPerson> {person1};
            var result = rep.FindByUsers(persons);
            Assert.AreEqual(1, result[person1].Count());
            Assert.AreEqual(false, result.ContainsKey(person2));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(result[person1].Find(abs).AccountCollection()));    
        }


        [Test]
        public void NoDuplicatePersonAbsences()
        {
            var person = createPersonInDb();
            var absence = createAbsenceInDb();
            var one = new PersonAbsenceAccount(person, absence);
            var two = new PersonAbsenceAccount(person, absence);
            try
            {
                CleanUpAfterTest();
                var rep = new PersonAbsenceAccountRepository(UnitOfWork);
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
            var rep = new PersonAbsenceAccountRepository(UnitOfWork);
            var dic = rep.LoadAllAccounts();
            var p = new Person();
            Assert.IsNotNull(dic[p]);
        }

        [Test]
        public void VerifyCanFindAccountsForOnePerson()
        {
            var account = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(account);

            var rep = new PersonAbsenceAccountRepository(UnitOfWork);
            var foundAccount = rep.Find(account.Person);
            
            Assert.IsNotNull(foundAccount);
            Assert.AreEqual(account.AccountCollection().Count(),foundAccount.AllPersonAccounts().Count());
        }
    }
}