using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker.LegacyTests
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