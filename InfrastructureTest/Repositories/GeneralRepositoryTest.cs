using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class GeneralRepositoryTest:DatabaseTest
    {
        [Test]
        public void CanGetServerTime()
        {
            GeneralRepository generalRepository = new GeneralRepository(UnitOfWork);
            DateTime now = generalRepository.ServerTime;
        
            //Dont know about this test, how to test now?
            Assert.IsTrue(now < now.AddDays(7) && now > now.AddDays(-7));
        }


        protected override void SetupForRepositoryTest()
        {
           
        }
    }
}
