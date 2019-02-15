using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MonitorSystem;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	[DomainTest]
	public class TryExecuteMonitorStepTest : IIsolateSystem
	{
		public TryExecuteMonitorStep Target;
		public FakeMonitorStep MonitorStep;
		
		[Test]
		public void ShouldExecuteMonitorStep()
		{
			var monitorStepResult = new MonitorStepResult(true, "something");
			MonitorStep.SetResult(monitorStepResult);

			var wasFound = Target.TryExecute(MonitorStep.Name, out var result);
			
			Assert.Multiple(() =>
			{
				wasFound.Should().Be.True();
				result.Should().Be.SameInstanceAs(monitorStepResult);				
			});
		}

		[Test]
		public void ShouldExecuteMonitorStepAlsoWhenCasingDiffers()
		{
			var wasFound1 = Target.TryExecute(MonitorStep.Name.ToUpper(), out _);
			var wasFound2 = Target.TryExecute(MonitorStep.Name.ToLower(), out _);
			
			Assert.Multiple(() =>
			{
				wasFound1.Should().Be.True();
				wasFound2.Should().Be.True();
			});		
		}

		[Test]
		public void ShouldReturnFalseIfStepNameDoesntExist()
		{
			Assert.Multiple(() =>
			{
				var wasFound = Target.TryExecute("non existing", out var result);
				wasFound.Should().Be.False();
				result.Should().Be.Null();
			});
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeMonitorStep>().For<IMonitorStep>();
		}
	}
}