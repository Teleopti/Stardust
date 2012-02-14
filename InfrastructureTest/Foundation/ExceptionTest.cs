using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [TestFixture]
    [Category("LongRunning")]
    public class ExceptionTest : DatabaseTest
    {
        protected override void SetupForRepositoryTest()
        {
        }

        [Test]
        public void VerifyViolatingUniqueIndex()
        {
            SkipRollback();
            bool ok = false;
            IPerson per1 = new Person();
            addLogonInfo(per1);
            IPerson per2 = new Person();
            addLogonInfo(per2);
            try
            {
                IPersonRepository rep = new PersonRepository(UnitOfWork);
                rep.Add(per1);
                rep.Add(per2);
                UnitOfWork.PersistAll();
            }
            catch (ConstraintViolationException ex)
            {
                ok = true;
                StringAssert.Contains("INSERT INTO Person", ex.Sql);
            }
            if(!ok)
                Assert.Fail("ConstraintViolationException was not thrown!");
        }


        private static void addLogonInfo(IPerson person)
        {
            person.PermissionInformation.SetDefaultTimeZone(TimeZoneHelper.CurrentSessionTimeZone);
            person.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName = "a";
            person.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName = "b";
        }
    }
}
