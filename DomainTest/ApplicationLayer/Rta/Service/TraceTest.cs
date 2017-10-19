using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	public class TraceTest
	{
		public FakeRtaDatabase Database;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public FakeRtaTracerPersister Logs;
		public IRtaTracer Tracer;
		public IDataSourceScope DataSource;

		[Test]
		public void ShouldTrace()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "statecode")
				.WithStateCode("statecode");
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Tracer.Trace("usercode");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Should().Not.Be.Empty();
		}
	}
}