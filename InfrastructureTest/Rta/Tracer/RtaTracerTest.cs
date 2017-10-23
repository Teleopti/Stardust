using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Tracer
{
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	public class RtaTracerTest
	{
		public IRtaTracer Tracer;
		public IRtaTracerReader Target;

		[Test]
		public void ShouldTrace()
		{
			Tracer.Trace("usercode");

			Tracer.StateReceived("usercode", "statecode");

			Target.ReadOfType<StateTraceLog>()
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldTrace2()
		{
			Tracer.Trace("usercode");

			Tracer.ActivityCheck(Guid.NewGuid());

			Target.ReadOfType<TracingLog>()
				.Should().Have.Count.EqualTo(1);
		}
		
		[Test]
		public void ShouldTraceProcessReceived()
		{
			Tracer.Trace("usercode");

			Tracer.ProcessReceived(null, null);

			Target.ReadOfType<TracingLog>()
				.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldStopTracing()
		{
			Assert.DoesNotThrow(() => Tracer.Stop());
		}
	}
}