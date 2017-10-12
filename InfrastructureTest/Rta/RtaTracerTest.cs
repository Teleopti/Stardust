using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	public class RtaTracerTest
	{
		public IRtaTracer Tracer;
		public IRtaTracerReader Target;
		public WithAnalyticsUnitOfWork Uow;

		[Test]
		public void ShouldTrace()
		{
			Tracer.Trace("usercode");

			Tracer.StateReceived("usercode", "statecode");

			Uow.Get(() => Target.ReadOfType<StateTraceLog>())
				.Should().Have.Count.EqualTo(1);
		}
		
		[Test]
		public void ShouldStopTracing()
		{
			Assert.DoesNotThrow(() => Tracer.Stop());
		}
		
	}
}