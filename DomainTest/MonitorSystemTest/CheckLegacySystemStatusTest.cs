using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MonitorSystem;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	[DomainTest]
	public class CheckLegacySystemStatusTest : IIsolateSystem
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

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeCallLegacySystemStatus>().For<ICallLegacySystemStatus>();
		}
	}
}