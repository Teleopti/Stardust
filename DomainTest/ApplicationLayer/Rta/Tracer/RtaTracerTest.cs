using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		public void ShouldLogStateReceivedWithProcess()
		{
			Target.Trace("usercode");

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Process.Should().Be(RtaTracer.ProcessName());
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

			Target.StateReceived("idontmatch", "statecode");

			Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldLogWhenUserCodeIsMatchedCaseInsensitive()
		{
			Target.Trace("UsErCoDe");

			Target.StateReceived("usercode", "statecode");

			Logs.ReadOfType<StateTraceLog>().Single().Log.Id.Should().Not.Be(Guid.Empty);
		}

		[Test]
		public void ShouldLogInvalidStateCode()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.InvalidStateCode(trace);

			var log = Logs.ReadOfType<StateTraceLog>().Single(x => x.Message == "Invalid state code");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.Should().Be.SameInstanceAs(trace);
		}

		[Test]
		public void ShouldNotLogInvalidStateCodeWhenUserCodeIsNotMatched()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("idontmatch", "statecode");
			Target.InvalidStateCode(trace);

			Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldLogStateProcessing()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.StateProcessing(trace);

			var log = Logs.ReadOfType<StateTraceLog>().Single(x => x.Message == "Processing");
			log.Process.Should().Be(RtaTracer.ProcessName());
			log.Log.Should().Be.SameInstanceAs(trace);
		}

		[Test]
		public void ShouldNotLogStateProcessingWhenUserCodeIsNotMatched()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("idontmatch", "statecode");
			Target.StateProcessing(trace);

			Logs.ReadOfType<StateTraceLog>().Should().Be.Empty();
		}

		[Test]
		public void ShouldLogInvalidAuthenticationKey()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.InvalidAuthenticationKey(trace);

			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain("Invalid authentication key");
		}

		[Test]
		public void ShouldLogInvalidSourceId()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.InvalidSourceId(trace);

			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain("Invalid source Id");
		}

		[Test]
		public void ShouldLogInvalidUserCode()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.InvalidUserCode(trace);

			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain("Invalid user code");
		}

		[Test]
		public void ShouldLogNoChange()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.NoChange(trace);

			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain("No change");
		}

		[Test]
		public void ShouldLogStateProcessed()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.StateProcessed(trace, null);

			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain("Processed");
		}

		[Test]
		public void ShouldLogStateProcessedWithEvents()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.StateProcessed(trace, new[] {new PersonStateChangedEvent()});

			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain(typeof(PersonStateChangedEvent).Name);
		}

		[Test]
		public void ShouldLogStateProcessedWith2Events()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			Target.StateProcessed(trace, new IEvent[] {new PersonStateChangedEvent(), new PersonActivityStartEvent()});

			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain(typeof(PersonStateChangedEvent).Name);
			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain(typeof(PersonActivityStartEvent).Name);
		}

		[Test]
		public void ShouldLogStateProcessedWithRandomEvent()
		{
			Target.Trace("usercode");

			var trace = Target.StateReceived("usercode", "statecode");
			var eventz = new IEvent[]
					{new PersonOutOfAdherenceEvent(), new PersonInAdherenceEvent(), new PersonNeutralAdherenceEvent()}.Randomize()
				.First();
			Target.StateProcessed(trace, new IEvent[] {eventz});

			Logs.ReadOfType<StateTraceLog>().Select(x => x.Message).Should().Contain(eventz.GetType().Name);
		}
	}
}