using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tracer;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Tracer
{
	[RtaTest]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerActivityCheckTraceTest
	{
		public FakeDatabase Database;
		public IRtaTracer Target;
		public FakeRtaTracerPersister Logs;
		public FakeRtaTracerConfigPersister Config;
		public IDataSourceScope DataSource;

		[Test]
		public void ShouldLogActivityCheck()
		{
			var person = Guid.NewGuid();
			var userCode = RandomName.Make();
			Database
				.WithAgent(userCode, person);
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.Trace(userCode);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.ActivityCheck(person);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Contain(userCode);
		}

		[Test]
		public void ShouldNotLogActivityCheckWhenNotTraced()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.ActivityCheck(person);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldLogActivityCheckForTracedUserCode()
		{
			var person = Guid.NewGuid();
			var userCode = RandomName.Make();
			var userCode2 = RandomName.Make();
			Database
				.WithAgent(userCode, person)
				.WithExternalLogon(userCode2)
				;
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.Trace(userCode2);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.ActivityCheck(person);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Contain(userCode2);
		}

		[Test]
		public void ShouldLogActivityCheckForTracedPerson()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithAgent("usercode1", person1)
				.WithAgent("usercode2", person2)
				;
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.Trace("usercode2");

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.ActivityCheck(person1);
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.ActivityCheck(person2);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Contain("usercode2");
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Not.Contain("usercode1");
		}

		[Test]
		public void ShouldLogMessage()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person);
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.Trace("usercode");

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.ActivityCheck(person);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Single().Message.Should().Be("Activity checked");
		}

		[Test]
		public void ShouldLogTracing()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithAgent("jågej kjax", person1)
				.WithExternalLogon("usercode")
				.WithAgent("pierre baldi", person2)
				.WithExternalLogon("usercode")
				;
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.Trace("usercode");

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.ActivityCheck(person1);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
			{
				var actual = Logs.ReadOfType<TracingLog>().Single();
				actual.Log.Tracing.Should().Contain("usercode");
				actual.Log.Tracing.Should().Contain("jågej");
				actual.Log.Tracing.Should().Contain("kjax");
				actual.Log.Tracing.Should().Contain("pierre");
				actual.Log.Tracing.Should().Contain("baldi");
			}
		}

		[Test]
		public void ShouldLogTracingUser()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithAgent("jågej kjax", person1)
				.WithExternalLogon("usercode")
				.WithAgent("pierre baldi", person2)
				.WithExternalLogon("usercode")
				;
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.Trace("usercode");

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.ActivityCheck(person1);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
			{
				var actual = Logs.ReadOfType<StateTraceLog>().Single();
				actual.Log.User.Should().Contain("usercode");
				actual.Log.User.Should().Contain("jågej");
				actual.Log.User.Should().Contain("kjax");
				actual.Log.User.Should().Contain("pierre");
				actual.Log.User.Should().Contain("baldi");
			}
		}
	}
}