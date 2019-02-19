using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Status;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Status
{
	[DomainTest]
	public class ListStatusStepsTest : IIsolateSystem
	{
		public ListStatusSteps Target;
		public FakeStatusStep FakeStatusStep;
		
		[Test]
		public void ShouldListMonitorStepNames()
		{
			FakeStatusStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute().Should().Contain(FakeStatusStep.Name);
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStatusStep>().For<IStatusStep>();
		}
	}
}