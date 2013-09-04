using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.WinCode.PeopleAdmin.Commands;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class AddPersonEnableCommandTest
    {
        private readonly IAddPersonEnableCommand _target = new AddPersonEnableCommand();

        [Test]
        public void ShouldReturnTrue()
        {
            _target.CanExecute().Should().Be.True();
        }
    }
}