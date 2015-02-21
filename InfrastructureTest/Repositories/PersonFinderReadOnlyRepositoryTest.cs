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
    public class PersonFinderReadOnlyRepositoryTest : DatabaseTest
    {
        private IPersonFinderReadOnlyRepository _target;

        [Test]
        public void ShouldLoadPersons()
        {
            UnitOfWork.PersistAll();
            SkipRollback();
            var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "hejhej", 10,
                                                             new DateOnly(2012, 1, 1), 1, 1);
            _target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
            _target.Find(crit);
            Assert.That(crit.TotalRows, Is.EqualTo(0));
        }

		[Test]
		public void ShouldCallUpdateReadModelWithoutCrash()
		{
			UnitOfWork.PersistAll();
			SkipRollback();
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.UpdateFindPerson(new[ ] {Guid.NewGuid()});
		}

		[Test]
		public void ShouldCallUpdateGroupingReadModelGroupPageWithoutCrash()
		{
			UnitOfWork.PersistAll();
			SkipRollback();
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
            _target.UpdateFindPersonData(new[] { Guid.NewGuid() });
		}

		[Test]
		public void ShouldHandleTooSmallDate()
		{
			UnitOfWork.PersistAll();
			SkipRollback();
			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "hejhej", 10,
																			 new DateOnly(1012, 1, 1), 1, 1);
			_target = new PersonFinderReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
			_target.Find(crit);
			Assert.That(crit.TotalRows, Is.EqualTo(0));
		}
    }
}