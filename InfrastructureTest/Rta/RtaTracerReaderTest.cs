using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	[AnalyticsDatabaseTest]
	public class RtaTracerReaderTest
	{
		public IRtaTracer Tracer;
		public IRtaTracerReader Target;

		[Test]
		public void ShouldRead()
		{
			Tracer.ProcessReceived();

			Target.Read().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReadProcessReceived()
		{
			Tracer.ProcessReceived();

			Target.Read().Single().Message.Should().Contain("ProcessReceived");
		}

		[Test]
		public void ShouldReadTime()
		{
			var now = DateTime.UtcNow.Utc();

			Tracer.ProcessReceived();

			Target.Read().Single().Time.Utc().Ticks.Should().Be.GreaterThan(now.Ticks);
		}
	}
}