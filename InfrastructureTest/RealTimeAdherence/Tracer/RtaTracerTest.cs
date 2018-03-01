using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tracer;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Tracer
{
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	[Setting("UseSafeRtaTracer", false)]
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