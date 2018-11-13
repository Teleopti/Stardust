using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.Test.Tracer
{
	[DomainTest]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerNoUserCodeTest
	{
		public IRtaTracer Target;
		public FakeRtaTracerPersister Logs;
		public FakeDatabase Database;
		public IDataSourceScope DataSource;

		[Test]
		public void ShouldNotThrowOnNullUserCode()
		{
			Target.Trace(null);

			Assert.DoesNotThrow(() => { Target.StateReceived("usercode", "statecode"); });
		}

		[Test]
		public void ShouldNotMatchNullUserCodeWithEmpty()
		{
			Target.Trace(null);

			Target.StateReceived("", "statecode");

			Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotThrowOnNullPersonMapping()
		{
			Target.Trace("usercode");

			Assert.DoesNotThrow(() => { Target.ActivityCheck(Guid.NewGuid()); });
		}

		[Test]
		public void ShouldTraceProcessWhenNoUserCode()
		{
			Target.Trace(null);

			Target.ProcessReceived(null, null);

			Logs.ReadOfType<ProcessReceivedLog>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotTraceStateWhenNoUserCode()
		{
			Target.Trace(null);

			Target.StateReceived(null, "statecode");

			Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}
		
		[Test]
		public void ShouldLogTracing()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			Database
				.WithDataSource(new StateForTest().SourceId)
				.WithAgent("jågej kjax", person1)
				.WithExternalLogon("usercode")
				.WithAgent("pierre baldi", person2)
				.WithExternalLogon("usercode")
				;
			Target.Trace(null);

			Target.ProcessReceived(null, null);

			using (DataSource.OnThisThreadUse(Database.TenantName()))
			{
				var actual = Logs.ReadOfType<TracingLog>().Single();
				actual.Log.Tracing.Should().Be.Null();
			}
		}
	}
}