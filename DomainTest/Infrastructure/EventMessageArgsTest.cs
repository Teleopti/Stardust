using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.DomainTest.Infrastructure
{
    [TestFixture]
    public class EventMessageArgsTest
    {
        [Test]
        public void ShouldCreateMessageArgs()
        {
            var message = MockRepository.GenerateMock<IEventMessage>();
            var target = new EventMessageArgs(message);
            target.Message.Should().Be.EqualTo(message);
        }
    }
}
