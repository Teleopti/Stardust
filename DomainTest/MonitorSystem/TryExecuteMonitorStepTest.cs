using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MonitorSystem;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MonitorSystem
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

			var result = Target.TryExecute(MonitorStep.Name);
			
			Assert.Multiple(() =>
			{
				result.Should().Be.SameInstanceAs(monitorStepResult);				
			});
		}

		[Test]
		public void ShouldExecuteMonitorStepAlsoWhenCasingDiffers()
		{
			var result1 = Target.TryExecute(MonitorStep.Name.ToUpper());
			var result2 = Target.TryExecute(MonitorStep.Name.ToLower());
			
			Assert.Multiple(() =>
			{
				result1.Success.Should().Be.True();
				result2.Success.Should().Be.True();
			});		
		}

		[Test]
		public void ShouldReturnFalseIfStepNameDoesntExist()
		{
			const string stepName = "non existing";
			Assert.Multiple(() =>
			{
				var result = Target.TryExecute(stepName);
				result.Success.Should().Be.False();
				result.Output.Should().Be.EqualTo(string.Format(Target.NonExistingStepName, stepName));
			});
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeMonitorStep>().For<IMonitorStep>();
		}
	}
}