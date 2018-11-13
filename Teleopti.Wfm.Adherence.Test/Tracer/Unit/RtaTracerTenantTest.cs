using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.Test.Tracer.Unit
{
	[RtaTest]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerTenantTest
	{
		public IRtaTracer Target;
		public FakeRtaTracerPersister Logs;
		public FakeTenants Tenants;
		public IDataSourceScope DataSource;
		public FakeDatabase Database;

		[Test]
		public void ShouldLogProcessReceivedWithoutTenant()
		{
			Tenants.Has("firstTenant");
			Tenants.Has("secondTenant");
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Trace("secondUserCode");

			using (DataSource.OnThisThreadUse(null as string))
				Target.ProcessReceived(null, null);

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<ProcessReceivedLog>().Single().Tenant.Should().Be.Null();
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<ProcessReceivedLog>().Single().Tenant.Should().Be.Null();
		}

		[Test]
		public void ShouldLogProcessReceivedForCurrentTenant()
		{
			Tenants.Has("firstTenant");
			Tenants.Has("secondTenant");
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Trace("secondUserCode");

			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.ProcessReceived(null, null);

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<ProcessReceivedLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<ProcessReceivedLog>().Should().Have.Count.EqualTo(0);
		}
		
		[Test]
		public void ShouldLogProcessProcessingForCurrentTenant()
		{
			Tenants.Has("firstTenant");
			Tenants.Has("secondTenant");
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Trace("secondUserCode");

			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.ProcessProcessing(null);

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldOnlyLogProcessProcessingForCurrentTenant()
		{
			Tenants.Has("firstTenant");
			Tenants.Has("secondTenant");
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Stop();

			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.ProcessProcessing(null);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.ProcessProcessing(null);

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldLogProcessActivityCheckForCurrentTenant()
		{
			Tenants.Has("firstTenant");
			Tenants.Has("secondTenant");
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Trace("secondUserCode");

			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.ProcessActivityCheck();

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<ProcessActivityCheckLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<ProcessActivityCheckLog>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldOnlyLogProcessActivityCheckForCurrentTenant()
		{
			Tenants.Has("firstTenant");
			Tenants.Has("secondTenant");
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Stop();

			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.ProcessActivityCheck();
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.ProcessActivityCheck();

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<ProcessActivityCheckLog>().Should().Have.Count.EqualTo(1);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<ProcessActivityCheckLog>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldLogStateReceivedForCurrentTenant()
		{
			Tenants.Has("firstTenant");
			Tenants.Has("secondTenant");
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Trace("secondUserCode");

			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.StateReceived("secondUserCode", "statecode");

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<StateTraceLog>().Should().Have.Count.EqualTo(0);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<StateTraceLog>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotLogStateReceivedForOtherTenant()
		{
			Tenants.Has("firstTenant");
			Tenants.Has("secondTenant");
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Trace("secondUserCode");

			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.StateReceived("firstUserCode", "statecode");

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<StateTraceLog>().Should().Have.Count.EqualTo(0);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<StateTraceLog>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldLogActivityCheckForCurrentTenant()
		{
			var person = Guid.NewGuid();
			Database
				.WithTenant("firstTenant")
				.WithTenant("secondTenant")
				.WithAgent("secondUserCode", person);
			using (DataSource.OnThisThreadUse("firstTenant"))
				Target.Trace("firstUserCode");
			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.Trace("secondUserCode");

			using (DataSource.OnThisThreadUse("secondTenant"))
				Target.ActivityCheck(person);

			using (DataSource.OnThisThreadUse("firstTenant"))
				Logs.ReadOfType<StateTraceLog>().Should().Have.Count.EqualTo(0);
			using (DataSource.OnThisThreadUse("secondTenant"))
				Logs.ReadOfType<StateTraceLog>().Should().Have.Count.EqualTo(1);
		}
	}
}