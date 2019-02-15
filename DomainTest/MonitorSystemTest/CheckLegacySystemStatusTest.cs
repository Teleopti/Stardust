using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	[MonitorTest]
	public class CheckLegacySystemStatusTest
	{
		public CheckLegacySystemStatus Target;
		public FakeCallLegacySystemStatus CallLegacySystemStatus;
		
		[Test]
		public void ShouldReturnOk()
		{
			var res = Target.Execute();
			
			res.Success.Should().Be.True();
			res.Output.Should().Be.EqualTo(CheckLegacySystemStatus.SuccessOutput);
		}
		

		[Test]
		public void ShouldReturnFailure()
		{
			CallLegacySystemStatus.WillFail();

			var res = Target.Execute();
			
			res.Success.Should().Be.False();
			res.Output.Should().Be.EqualTo(CheckLegacySystemStatus.FailureOutput);
		}
	}
}