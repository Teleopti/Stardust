using System;
using NUnit.Framework;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class ApplicationRolePersonRepositoryTest :DatabaseTest
    {
        private ApplicationRolePersonRepository _target;


        [Test]
        public void ShouldLoadPersonsInRole()
        {
            var roleId = Guid.NewGuid();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var persons = _target.GetPersonsInRole(roleId);
                Assert.That(persons.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadPersonsNotInRole()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            var roleId = Guid.NewGuid();
            var person1Id = Guid.NewGuid();
            var person2Id = Guid.NewGuid();
            var personsIds = new List<Guid> {person1Id, person2Id};

            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var persons = _target.GetPersonsNotInRole(roleId, personsIds);
                Assert.That(persons.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadPersons()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var persons = _target.Persons();
                Assert.That(persons, Is.Not.Null);
            }
        }

        [Test]
        public void ShouldLoadRolesOnPerson()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.RolesOnPerson(Guid.NewGuid());
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadFunctionsOnPerson()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.FunctionsOnPerson(Guid.NewGuid());
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadFunctions()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.Functions();
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadPersonsOnFunction()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.PersonsWithFunction(Guid.NewGuid());
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadRolesOnFunction()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.RolesWithFunction(Guid.NewGuid());
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadDataOnPerson()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.DataRangeOptions(Guid.NewGuid());
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadDataRangeOptionsOnPerson()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.AvailableData(Guid.NewGuid());
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadRolesWithData()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.RolesWithData(Guid.NewGuid());
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadPersonsOnRoles()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.PersonsWithRoles(new List<Guid>{Guid.NewGuid(), Guid.NewGuid()});
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadRolesWithDataRange()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new ApplicationRolePersonRepository(uow);
                var result = _target.RolesWithDataRange(1);
                Assert.That(result.Count, Is.EqualTo(0));
            }
        }
    }

}