using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;

namespace Teleopti.Ccc.DomainTest.MessageBroker.ImplementationDetailTests
{
	[TestFixture]
	public class ActionImmediateTest
	{
		[Test]
		public void ShouldExecuteAction()
		{
			var executed = false;
			var target = new ActionImmediate();

			target.Do(() => { executed = true; });

			executed.Should().Be.True();
		}
	}
}