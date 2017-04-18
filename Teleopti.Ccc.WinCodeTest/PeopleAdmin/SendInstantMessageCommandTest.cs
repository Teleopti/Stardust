using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class SendInstantMessageCommandTest
    {
        private ISendInstantMessageCommand _target;

        [SetUp]
        public void Setup()
        {
            _target = new SendInstantMessageCommand();
        }

        [Test]
        public void ShouldReturnFalseIfNotAllowed()
        {
            using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
                Assert.That(_target.CanExecute(), Is.False);
            }
        }

        [Test]
        public void ShouldReturnTrueIfAllowed()
        {
            Assert.That(_target.CanExecute(), Is.True);
        }
    }


}