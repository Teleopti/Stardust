using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	[Ignore("Implementation was experimental")]
	public class RtaTracerReaderTest
	{
		public IRtaTracer Tracer;
		public IRtaTracerReader Target;

		[Test]
		public void ShouldRead()
		{
			Tracer.ProcessReceived();

			Target.ReadOfType<ProcessReceivedLog>().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReadProcessReceived()
		{
			Tracer.ProcessReceived();

			Target.ReadOfType<ProcessReceivedLog>().Single().Message.Should().Contain("Data received at");
		}

		[Test]
		public void ShouldReadTime()
		{
			var now = DateTime.UtcNow.Utc();

			Tracer.ProcessReceived();

			Target.ReadOfType<ProcessReceivedLog>().Single().Time.Utc().Ticks.Should().Be.GreaterThan(now.Ticks);
		}
		
	}
}