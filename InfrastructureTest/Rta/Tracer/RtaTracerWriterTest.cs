using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Tracer
{
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	public class RtaTracerWriterTest
	{
		public IRtaTracerWriter Target;
		public IRtaTracerReader Reader;
		public MutableNow Now;
		public ICurrentDataSource DataSource;

		[Test]
		public void ShouldWrite()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = DataSource.CurrentName()});

			var actual = Reader.ReadOfType<ProcessReceivedLog>();
			actual.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldWriteProcessReceivedLog()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog>
			{
				Message = "message",
				Tenant = DataSource.CurrentName(),
				Log = new ProcessReceivedLog {ReceivedAt = "2017-10-11 10:00".Utc()}
			});

			var actual = Reader.ReadOfType<ProcessReceivedLog>().Single();
			actual.Message.Should().Be("message");
			actual.Log.ReceivedAt.Should().Be("2017-10-11 10:00".Utc());
		}

		[Test]
		public void ShouldWriteTime()
		{
			var now = Now.UtcDateTime();
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = DataSource.CurrentName()});

			var actual = Reader.ReadOfType<ProcessReceivedLog>().Single();
			actual.Time.Should().Be.GreaterThanOrEqualTo(now);
		}

		[Test]
		public void ShouldWrite2DifferentLogs()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = DataSource.CurrentName()});
			Target.Write(new RtaTracerLog<ProcessProcessingLog> {Tenant = DataSource.CurrentName()});

			Reader.ReadOfType<ProcessReceivedLog>().Should().Have.Count.EqualTo(1);
			Reader.ReadOfType<ProcessProcessingLog>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldWriteAHugeMessage()
		{
			var message = new string('x', short.MaxValue);
			Target.Write(new RtaTracerLog<StateTraceLog> {Tenant = DataSource.CurrentName(), Message = message});

			Reader.ReadOfType<StateTraceLog>().Single().Message.Should().Be(message);
		}

		[Test]
		public void ShouldClear()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog> {Tenant = DataSource.CurrentName()});

			Target.Clear();

			Reader.ReadOfType<ProcessReceivedLog>().Should().Be.Empty();
		}
	}
}