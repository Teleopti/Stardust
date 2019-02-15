using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MonitorSystem;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	[DomainTest]
	public class ListMonitorStepsTest : IIsolateSystem
	{
		public ListMonitorSteps Target;
		public FakeMonitorStep FakeMonitorStep;
		
		[Test]
		public void ShouldListMonitorStepNames()
		{
			FakeMonitorStep.Name = Guid.NewGuid().ToString();
			
			Target.Execute().Should().Contain(FakeMonitorStep.Name);
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeMonitorStep>().For<IMonitorStep>();
		}
	}
}