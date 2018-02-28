using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Rta.Service;
using Teleopti.Ccc.Domain.Rta.Tracer;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Setting("UseSafeRtaTracer", false)]
	public class TraceTest
	{
		public FakeDatabase Database;
		public Domain.Rta.Service.Rta Target;
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

		[Test]
		public void ShouldTracePersonStateChangedEvent()
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
				Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain(nameof(PersonStateChangedEvent));
		}

		[Test]
		public void ShouldTraceInvalidAuthenticationKey()
		{
			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Tracer.Trace("usercode");

			try
			{
				Target.ProcessState(new StateForTest
				{
					AuthenticationKey = "key"
				});
			}
			catch
			{
			}

			using (DataSource.OnThisThreadUse(Database.TenantName()))
				Logs.ReadOfType<ProcessExceptionLog>().Select(x => x.Log.Type).Should().Contain(nameof(InvalidAuthenticationKeyException));
		}
	}
}