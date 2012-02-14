using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class InvalidRequestStateTransitionExceptionTest
    {
        private InvalidRequestStateTransitionException target;

        [SetUp]
        public void Setup()
        {
            target = new InvalidRequestStateTransitionException();
        }

        [Test]
        public void ShouldHandleInnerException()
        {
            target = new InvalidRequestStateTransitionException("test",target);
            Assert.IsNotNull(target.InnerException);
        }
    }
}
