using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Status
{
	[DomainTest]
	public class ExecuteMonitorStepTest : IIsolateSystem
	{
		public ExecuteStatusStep Target;
		public FakeStatusStep StatusStep;
		
		[Test]
		public void ShouldExecuteMonitorStep()
		{
			var monitorStepResult = new StatusStepResult(true, "something");
			StatusStep.SetResult(monitorStepResult);

			Target.Execute(StatusStep.Name)
				.Should().Be.SameInstanceAs(monitorStepResult);				
		}

		[Test]
		public void ShouldExecuteMonitorStepAlsoWhenCasingDiffers()
		{
			var result1 = Target.Execute(StatusStep.Name.ToUpper());
			var result2 = Target.Execute(StatusStep.Name.ToLower());
			
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
				var result = Target.Execute(stepName);
				result.Success.Should().Be.False();
				result.Output.Should().Be.EqualTo(string.Format(Target.NonExistingStepName, stepName));
			});
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStatusStep>().For<IStatusStep>();
		}
	}
}