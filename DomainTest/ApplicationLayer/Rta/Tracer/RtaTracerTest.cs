using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Tracer
{
	[DomainTest]
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	public class RtaTracerTest
	{
		public IRtaTracer Target;
		public FakeKeyValueStorePersister KeyValueStore;
		public FakeRtaTracerPersister Logs;

		[Test]
		public void ShouldLogStateReceived()
		{
			KeyValueStore.Update("RtaTracerUserCode", "usercode");

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Log.StateCode.Should().Be("statecode");
			Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Be("usercode");
		}

		[Test]
		public void ShouldLogStateReceivedWithId()
		{
			KeyValueStore.Update("RtaTracerUserCode", "usercode");

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Log.Id.Should().Not.Be(Guid.Empty);
		}

		[Test]
		public void ShouldLogStateReceivedWithMessage()
		{
			Target.Trace("usercode");

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Message.Should().Be("Received");
		}

		[Test]
		public void ShouldLogStateReceivedWithBoxName()
		{
			Target.Trace("usercode");

			Target.StateReceived("usercode", "statecode");

			var boxName = Environment.GetEnvironmentVariable("COMPUTERNAME")
						   ?? Environment.GetEnvironmentVariable("HOSTNAME");
			Logs.ReadOfType<StateTraceLog>().Single().Process.Should().Contain(boxName);
		}

		[Test]
		public void ShouldLogStateReceivedWithProcessId()
		{
			Target.Trace("usercode");

			Target.StateReceived("usercode", "statecode");

			var processId = Process.GetCurrentProcess().Id.ToString();			
			Logs.ReadOfType<StateTraceLog>().Single().Process.Should().Contain(processId);
		}

		[Test]
		public void ShouldReturnStateTraceLog()
		{
			KeyValueStore.Update("RtaTracerUserCode", "usercode");

			var result = Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Log.Should().Be.SameInstanceAs(result);
		}

		[Test]
		public void ShouldNotLogWhenStopped()
		{
			KeyValueStore.Update("RtaTracerUserCode", "usercode");
			Target.Stop();

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldLogWhenStartedByOtherProcess()
		{
			Target.Stop();
			KeyValueStore.Update("RtaTracerUserCode", "usercode");

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Log.Id.Should().Not.Be(Guid.Empty);
		}

		[Test]
		public void ShouldLogWhenStarted()
		{
			Target.Stop();
			Target.Trace("usercode");

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Log.Id.Should().Not.Be(Guid.Empty);
		}

		[Test]
		public void ShouldNotLogWhenUserCodeIsNotMatched()
		{
			Target.Trace("usercode");

			Target.StateReceived("usercode2", "statecode");

			Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldLogWhenUserCodeIsMatchedCaseInsensitive()
		{
			Target.Trace("UsErCoDe");

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Log.Id.Should().Not.Be(Guid.Empty);
		}
	}
}