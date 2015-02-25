using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture, Category("LongRunning")]
    public class PersonRequestReadOnlyRepositoryTest : DatabaseTest
    {
        private IRequestHistoryReadOnlyRepository _target;

        [Test]
        public void ShouldLoadRequestWithoutCrash()
        {
            UnitOfWork.PersistAll();
            CleanUpAfterTest();

            _target = new RequestHistoryReadOnlyRepository(UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork());
            var result = _target.LoadOnPerson(new Guid(),1, 10 );
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}