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

namespace Teleopti.Wfm.Adherence.Test.Tracer
{
	[RtaTest]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerCloseSnapshotTraceTest
	{
		public FakeDatabase Database;
		public IRtaTracer Target;
		public FakeRtaTracerPersister Logs;
		public FakeRtaTracerConfigPersister Config;
		public IDataSourceScope DataSource;

		[Test]
		public void ShouldLogSnapshotLogout()
		{
			var person = Guid.NewGuid();
			var userCode = RandomName.Make();
			Database
				.WithAgent(userCode, person);
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.Trace(userCode);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.SnapshotLogout(person, Ccc.Domain.RealTimeAdherence.Domain.Service.Rta.LogOutBySnapshot);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Contain(userCode);
		}

		[Test]
		public void ShouldLogStateCode()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person);
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.Trace("usercode");

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Target.SnapshotLogout(person, Ccc.Domain.RealTimeAdherence.Domain.Service.Rta.LogOutBySnapshot);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Single().Log.StateCode.Should().Be(Ccc.Domain.RealTimeAdherence.Domain.Service.Rta.LogOutBySnapshot);
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
				Target.SnapshotLogout(person, Ccc.Domain.RealTimeAdherence.Domain.Service.Rta.LogOutBySnapshot);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<StateTraceLog>().Single().Message.Should().Be("Snapshot logout");
		}
		
	}
}