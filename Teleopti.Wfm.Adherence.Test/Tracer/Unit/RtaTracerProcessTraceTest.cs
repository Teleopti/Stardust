using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.Test.Tracer.Unit
{
	[DomainTest]
	[Setting("UseSafeRtaTracer", false)]
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

			Target.ProcessReceived(null, null);

			var log = Logs.ReadOfType<ProcessReceivedLog>().Single();
			log.Message.Should().Be("Data received at");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.At.Should().Be("2017-10-09 13:00".Utc());
		}

		[Test]
		public void ShouldLogProcessReceivedByAndCount()
		{
			Now.Is("2017-10-09 13:00");
			Target.Trace("usercode");

			Target.ProcessReceived("method", 123);

			var log = Logs.ReadOfType<ProcessReceivedLog>().Single();
			log.Log.By.Should().Be("method");
			log.Log.Count.Should().Be(123);
		}

		[Test]
		public void ShouldLogProcessProcessing()
		{
			Now.Is("2017-10-09 13:00");
			Target.Trace("usercode");

			Target.ProcessProcessing(3);

			var log = Logs.ReadOfType<ProcessProcessingLog>().Single();
			log.Message.Should().Be("Processing");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.Count.Should().Be(3);
			log.Log.At.Should().Be("2017-10-09 13:00".Utc());
		}

		[Test]
		public void ShouldLogProcessEnqueuing()
		{
			Now.Is("2017-10-09 13:00");
			Target.Trace("usercode");

			Target.ProcessEnqueuing(4);

			var log = Logs.ReadOfType<ProcessEnqueuingLog>().Single();
			log.Message.Should().Be("Enqueuing");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.Count.Should().Be(4);
			log.Log.At.Should().Be("2017-10-09 13:00".Utc());
		}
		
		[Test]
		public void ShouldLogActivityCheck()
		{
			Now.Is("2017-10-09 13:00");
			Target.Trace("usercode");

			Target.ProcessActivityCheck();

			var log = Logs.ReadOfType<ProcessActivityCheckLog>().Single();
			log.Message.Should().Be("Activity check at");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.At.Should().Be("2017-10-09 13:00".Utc());
		}

		[Test]
		public void ShouldLogProcessException()
		{
			Target.Trace("usercode");

			Exception exception;
			try
			{
				throw new InvalidAuthenticationKeyException("blip blop");
			}
			catch (Exception e)
			{
				Target.ProcessException(e);
				exception = e;
			}

			var log = Logs.ReadOfType<ProcessExceptionLog>().Single();
			log.Message.Should().Be("blip blop");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.Type.Should().Be(nameof(InvalidAuthenticationKeyException));
			log.Log.Info.Should().Contain(exception.Message);
			log.Log.Info.Should().Contain(exception.StackTrace);
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

			Target.ProcessReceived(null, null);

			Logs.ReadOfType<ProcessReceivedLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotLogProcessProcessingWhenStopped()
		{
			Target.Trace("usercode");
			Target.Stop();

			Target.ProcessProcessing(null);

			Logs.ReadOfType<ProcessProcessingLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotLogActivityCheckWhenStopped()
		{
			Target.Trace("usercode");
			Target.Stop();

			Target.ProcessActivityCheck();

			Logs.ReadOfType<ProcessActivityCheckLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldLogTracing()
		{
			Now.Is("2017-10-12 13:00");
			Target.Trace("usercode");

			Target.ProcessReceived(null, null);

			var log = Logs.ReadOfType<TracingLog>().Single();
			log.Time.Should().Be("2017-10-12 13:00".Utc());
			log.Message.Should().Be("Tracing");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.Tracing.Should().Contain("usercode");
		}

		[Test]
		public void ShouldClearLogs()
		{
			Target.Trace("usercode");
			Target.ProcessReceived(null, null);

			Target.Clear();

			Logs.ReadOfType<ProcessReceivedLog>().Should().Be.Empty();
		}
	}
}