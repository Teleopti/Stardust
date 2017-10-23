using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Tracer
{
	[DomainTest]
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	public class RtaTracerProcessTraceTest
	{
		public IRtaTracer Target;
		public FakeRtaTracerPersister Logs;
		public MutableNow Now;

		[Test]
		public void ShouldLogProcessReceived()
		{
			Now.Is("2017-10-09 13:00");
			Target.Trace("usercode");

			Target.ProcessReceived();

			var log = Logs.ReadOfType<ProcessReceivedLog>().Single();
			log.Message.Should().Be("Data received at");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.RecievedAt.Should().Be("2017-10-09 13:00".Utc());
		}

		[Test]
		public void ShouldLogProcessProcessing()
		{
			Now.Is("2017-10-09 13:00");
			Target.Trace("usercode");

			Target.ProcessProcessing();

			var log = Logs.ReadOfType<ProcessProcessingLog>().Single();
			log.Message.Should().Be("Processing");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.ProcessingAt.Should().Be("2017-10-09 13:00".Utc());
		}

		[Test]
		public void ShouldLogActivityCheck()
		{
			Now.Is("2017-10-09 13:00");
			Target.Trace("usercode");

			Target.ProcessActivityCheck();

			var log = Logs.ReadOfType<ActivityCheckLog>().Single();
			log.Message.Should().Be("Activity check at");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.ActivityCheckAt.Should().Be("2017-10-09 13:00".Utc());
		}

		[Test]
		public void ShouldLogProcessException()
		{
			Target.Trace("usercode");

			Target.ProcessException(new InvalidAuthenticationKeyException("blip blop"));

			var log = Logs.ReadOfType<ProcessExceptionLog>().Single();
			log.Message.Should().Be("blip blop");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.Type.Should().Be(nameof(InvalidAuthenticationKeyException));
		}

		[Test]
		public void ShouldLogAnyProcessException()
		{
			Target.Trace("usercode");

			Target.ProcessException(new NullReferenceException());

			Logs.ReadOfType<ProcessExceptionLog>().Single()
				.Log.Type.Should().Be(nameof(NullReferenceException));
		}

		[Test]
		public void ShouldNotLogProcessReceivedWhenStopped()
		{
			Target.Trace("usercode");
			Target.Stop();

			Target.ProcessReceived();

			Logs.ReadOfType<ProcessReceivedLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotLogProcessProcessingWhenStopped()
		{
			Target.Trace("usercode");
			Target.Stop();

			Target.ProcessProcessing();

			Logs.ReadOfType<ProcessProcessingLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotLogActivityCheckWhenStopped()
		{
			Target.Trace("usercode");
			Target.Stop();

			Target.ProcessActivityCheck();

			Logs.ReadOfType<ActivityCheckLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldLogTracing()
		{
			Now.Is("2017-10-12 13:00");
			Target.Trace("usercode");

			Target.ProcessReceived();

			var log = Logs.ReadOfType<TracingLog>().Single();
			log.Message.Should().Be("Tracing");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.Tracing.Should().Contain("usercode");
		}

		[Test]
		public void ShouldClearLogs()
		{
			Target.Trace("usercode");
			Target.ProcessReceived();

			Target.Clear();

			Logs.ReadOfType<ProcessReceivedLog>().Should().Be.Empty();
		}
	}
}