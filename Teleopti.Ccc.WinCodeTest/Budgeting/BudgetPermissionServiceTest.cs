using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Budgeting
{
    [TestFixture]
    public class BudgetPermissionServiceTest
    {
        private IBudgetPermissionService target;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            target = mocks.StrictMock<BudgetPermissionService>();
        }

        [Test]
        public void ShouldShowNoPermission()
        {
            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
                Assert.IsFalse(target.IsAllowancePermitted);
            }
        }

        [Test]
        public void ShouldShowWithPermission()
        {
            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithFullPermission()))
            {
                Assert.IsTrue(target.IsAllowancePermitted);
            }
        }
    }
}
