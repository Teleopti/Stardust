using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	[AnalyticsDatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	public class RtaTracerWriterTest
	{
		public IRtaTracerWriter Target;
		public IRtaTracerReader Reader;
		public WithAnalyticsUnitOfWork Uow;
		public MutableNow Now;

		[Test]
		public void ShouldWrite()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog>());

			var actual = Uow.Get(() => Reader.ReadOfType<ProcessReceivedLog>());
			actual.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldWriteProcessReceivedLog()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog>
			{
				Message = "message",
				Log = new ProcessReceivedLog {RecievedAt = "2017-10-11 10:00".Utc()}
			});

			var actual = Uow.Get(() => Reader.ReadOfType<ProcessReceivedLog>()).Single();
			actual.Message.Should().Be("message");
			actual.Log.RecievedAt.Should().Be("2017-10-11 10:00".Utc());
		}

		[Test]
		public void ShouldWriteTime()
		{
			var now = Now.UtcDateTime();
			Target.Write(new RtaTracerLog<ProcessReceivedLog>());

			var actual = Uow.Get(() => Reader.ReadOfType<ProcessReceivedLog>()).Single();
			actual.Time.Should().Be.GreaterThanOrEqualTo(now);
		}

		[Test]
		public void ShouldWrite2DifferentLogs()
		{
			Target.Write(new RtaTracerLog<ProcessReceivedLog>());
			Target.Write(new RtaTracerLog<ProcessProcessingLog>());

			Uow.Get(() => Reader.ReadOfType<ProcessReceivedLog>()).Should().Have.Count.EqualTo(1);
			Uow.Get(() => Reader.ReadOfType<ProcessProcessingLog>()).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldWriteAHugeMessage()
		{
			var message = new string('x', short.MaxValue);
			Target.Write(new RtaTracerLog<StateTraceLog> {Message = message});

			Uow.Get(() => Reader.ReadOfType<StateTraceLog>()).Single().Message.Should().Be(message);
		}
	}
}