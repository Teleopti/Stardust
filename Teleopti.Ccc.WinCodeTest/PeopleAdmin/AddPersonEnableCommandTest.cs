using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class AddPersonEnableCommandTest
    {
        [Test]
        public void ShouldReturnTrue()
        {
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var target = new AddPersonEnableCommand();

				target.CanExecute().Should().Be.True();
			}
		}
    }
}