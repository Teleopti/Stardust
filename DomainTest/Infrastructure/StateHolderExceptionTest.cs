using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.DomainTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.DomainTest.Infrastructure
{
    /// <summary>
    /// Tests for StateHolderException
    /// </summary>
    [TestFixture]
    public class StateHolderExceptionTest : ExceptionTest<StateHolderException>
    {
        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns></returns>
        protected override StateHolderException CreateTestInstance(string message, Exception innerException)
        {
            return new StateHolderException(message, innerException);
        }

        /// <summary>
        /// Creates the test instance.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected override StateHolderException CreateTestInstance(string message)
        {
            return new StateHolderException(message);
        }
    }

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