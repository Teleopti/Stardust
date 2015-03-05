using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests UserDetailRepository
    ///</summary>
    [TestFixture]
    [Category("LongRunning")]
    public class UserDetailRepositoryTest : RepositoryTest<IUserDetail>
    {
        private IPerson person;

        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IUserDetail CreateAggregateWithCorrectBusinessUnit()
        {
            person = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(person);

            IUserDetail userDetail = new UserDetail(person);
            userDetail.RegisterPasswordChange();
            userDetail.RegisterInvalidAttempt(new DummyPasswordPolicy());
            userDetail.Lock();
            return userDetail;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IUserDetail loadedAggregateFromDatabase)
        {
            IUserDetail org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.InvalidAttempts, loadedAggregateFromDatabase.InvalidAttempts);
            Assert.AreEqual(org.InvalidAttemptsSequenceStart.Date, loadedAggregateFromDatabase.InvalidAttemptsSequenceStart.Date);
            Assert.AreEqual(org.IsLocked, loadedAggregateFromDatabase.IsLocked);
            Assert.AreEqual(org.LastPasswordChange.Date, loadedAggregateFromDatabase.LastPasswordChange.Date);
            Assert.AreEqual(org.Person.Name, loadedAggregateFromDatabase.Person.Name);
        }

        [Test]
        public void VerifyCanFindByPerson()
        {
            IUserDetail userDetail = CreateAggregateWithCorrectBusinessUnit(); 
            PersistAndRemoveFromUnitOfWork(userDetail);

            userDetail = new UserDetailRepository(UnitOfWork).FindByUser(person);
            Assert.IsNotNull(userDetail);
        }

        [Test]
        public void VerifyCanFindByNotExistingPerson()
        {
            person = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(person);

            IUserDetail userDetail = new UserDetailRepository(UnitOfWork).FindByUser(person);
            Assert.IsNotNull(userDetail);
        }

        [Test]
        public void VerifyCanFindAllPersons()
        {
            var p = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(p);

            var u = new UserDetail(p);
            u.RegisterPasswordChange();
            u.RegisterInvalidAttempt(new DummyPasswordPolicy());
            u.Lock();
            PersistAndRemoveFromUnitOfWork(u);

            var p1 = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(p1);

            var u1 = new UserDetail(p1);
            u1.RegisterPasswordChange();
            u1.RegisterInvalidAttempt(new DummyPasswordPolicy());
            u1.Lock();
            PersistAndRemoveFromUnitOfWork(u1);

            var users = new UserDetailRepository(UnitOfWork).FindAllUsers();

            Assert.IsNotNull(users.ContainsKey(p));
            Assert.IsNotNull(users.ContainsKey(p1));
        }

        [Test]
        public void ShouldFindByUsers()
        {
            var p = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(p);

            var u = new UserDetail(p);
            u.RegisterPasswordChange();
            u.RegisterInvalidAttempt(new DummyPasswordPolicy());
            u.Lock();
            PersistAndRemoveFromUnitOfWork(u);

            var p1 = PersonFactory.CreatePerson();
            PersistAndRemoveFromUnitOfWork(p1);

            var u1 = new UserDetail(p1);
            u1.RegisterPasswordChange();
            u1.RegisterInvalidAttempt(new DummyPasswordPolicy());
            u1.Lock();
            PersistAndRemoveFromUnitOfWork(u1);

            IList<IPerson> persons = new List<IPerson>{p1};
            var users = new UserDetailRepository(UnitOfWork).FindByUsers(persons);

            Assert.IsFalse(users.ContainsKey(p));
            Assert.IsTrue(users.ContainsKey(p1));    
        }

        protected override Repository<IUserDetail> TestRepository(IUnitOfWork unitOfWork)
        {
            return new UserDetailRepository(unitOfWork);
        }

    }
}
