using NUnit.Framework;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
    public class HandlingOfUowTest
    {
        private IUnitOfWorkFactory uowFactory;

        [SetUp]
        public void Setup()
        {
            uowFactory = SetupFixtureForAssembly.DataSource.Application;
        }


        [Test]
        public void CanFetchCurrentUnitOfWork()
        {
            using (var uow = uowFactory.CreateAndOpenUnitOfWork())
            {
                Assert.AreEqual(uow, uowFactory.CurrentUnitOfWork());
                using (var uow2 = uowFactory.CreateAndOpenUnitOfWork())
                {
                    Assert.AreEqual(uow2, uowFactory.CurrentUnitOfWork());
                    Assert.AreNotEqual(uow, uow2);
                }
            }
        }
    }
}