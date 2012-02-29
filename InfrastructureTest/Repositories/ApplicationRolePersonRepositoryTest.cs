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
            SkipRollback();
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
    }

    
}