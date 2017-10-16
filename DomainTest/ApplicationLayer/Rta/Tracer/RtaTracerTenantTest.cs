using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Tracer
{
	[DomainTest]
	[LoggedOff]
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	public class RtaTracerTenantTest
	{
		public IRtaTracer Target;
		public FakeRtaTracerPersister Logs;
		public FakeTenants Tenants;
		public IDataSourceScope DataSource;

		[Test]
		public void ShouldLogProcessReceivedForAllTenants()
		{
			Tenants.Has("tenant1");
			Tenants.Has("tenant2");
			using (DataSource.OnThisThreadUse("tenant1"))
				Target.Trace("usercode1");
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.Trace("usercode2");

			Target.ProcessReceived();

			using (DataSource.OnThisThreadUse("tenant1"))
				Logs.ReadOfType<ProcessReceivedLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("tenant2"))
				Logs.ReadOfType<ProcessReceivedLog>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldLogProcessProcessingForCurrentTenant()
		{
			Tenants.Has("tenant1");
			Tenants.Has("tenant2");
			using (DataSource.OnThisThreadUse("tenant1"))
				Target.Trace("usercode1");
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.Trace("usercode2");

			using (DataSource.OnThisThreadUse("tenant1"))
				Target.ProcessProcessing();

			using (DataSource.OnThisThreadUse("tenant1"))
				Logs.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("tenant2"))
				Logs.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldOnlyLogProcessProcessingForCurrentTenant()
		{
			Tenants.Has("tenant1");
			Tenants.Has("tenant2");
			using (DataSource.OnThisThreadUse("tenant1"))
				Target.Trace("usercode1");
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.Stop();

			using (DataSource.OnThisThreadUse("tenant1"))
				Target.ProcessProcessing();
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.ProcessProcessing();

			using (DataSource.OnThisThreadUse("tenant1"))
				Logs.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("tenant2"))
				Logs.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldLogProcessActivityCheckForCurrentTenant()
		{
			Tenants.Has("tenant1");
			Tenants.Has("tenant2");
			using (DataSource.OnThisThreadUse("tenant1"))
				Target.Trace("usercode1");
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.Trace("usercode2");

			using (DataSource.OnThisThreadUse("tenant1"))
				Target.ProcessActivityCheck();

			using (DataSource.OnThisThreadUse("tenant1"))
				Logs.ReadOfType<ActivityCheckLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("tenant2"))
				Logs.ReadOfType<ActivityCheckLog>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldOnlyLogProcessActivityCheckForCurrentTenant()
		{
			Tenants.Has("tenant1");
			Tenants.Has("tenant2");
			using (DataSource.OnThisThreadUse("tenant1"))
				Target.Trace("usercode1");
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.Stop();

			using (DataSource.OnThisThreadUse("tenant1"))
				Target.ProcessActivityCheck();
			using (DataSource.OnThisThreadUse("tenant2"))
				Target.ProcessActivityCheck();

			using (DataSource.OnThisThreadUse("tenant1"))
				Logs.ReadOfType<ActivityCheckLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("tenant2"))
				Logs.ReadOfType<ActivityCheckLog>().Should().Have.Count.EqualTo(0);
		}
	}
}