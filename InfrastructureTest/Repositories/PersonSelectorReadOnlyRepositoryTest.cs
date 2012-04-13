using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class PersonSelectorReadOnlyRepositoryTest : DatabaseTest
    {
        private PersonSelectorReadOnlyRepository _target;

        [Test]
        public void ShouldLoadGroupPages()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new PersonSelectorReadOnlyRepository(uow);
                var pages = _target.GetUserDefinedTabs();
                Assert.That(pages.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadOrganization()
        {
            UnitOfWork.PersistAll();
            SkipRollback();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new PersonSelectorReadOnlyRepository(uow);
                var nodes = _target.GetOrganization(new DateOnly(2012,1,27),true );
                Assert.That(nodes, Is.Not.Null);
            }
        }

        [Test]
        public void ShouldLoadBuiltIn()
        {
            UnitOfWork.PersistAll();
            SkipRollback();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new PersonSelectorReadOnlyRepository(uow);
                var nodes = _target.GetBuiltIn(new DateOnly(2012, 1, 27), PersonSelectorField.Contract);
                Assert.That(nodes.Count, Is.EqualTo(0));
            }
        }

        [Test]
        public void ShouldLoadUserTabs()
        {
            UnitOfWork.PersistAll();
            SkipRollback();
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                _target = new PersonSelectorReadOnlyRepository(uow);
                var nodes = _target.GetUserDefinedTab(new DateOnly(2012, 1, 27), Guid.NewGuid());
                Assert.That(nodes.Count, Is.EqualTo(0));
            }
        }
    }

}