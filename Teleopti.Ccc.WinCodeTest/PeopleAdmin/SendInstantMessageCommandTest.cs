using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.PeopleAdmin.Commands;

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
            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
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