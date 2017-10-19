using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Tracer
{
	[RtaTest]
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	public class RtaTracerActivityCheckTraceTest
	{
		public FakeRtaDatabase Database;
		public IRtaTracer Target;
		public FakeRtaTracerPersister Logs;
		public FakeRtaTracerConfigPersister Config;

		[Test]
		public void ShouldLogActivityCheck()
		{
			var person = Guid.NewGuid();
			var userCode = RandomName.Make();
			Database
				.WithAgent(userCode, person);
			Target.Trace(userCode);

			Target.ActivityCheck(person);

			Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Contain(userCode);
		}

		[Test]
		public void ShouldNotLogActivityCheckWhenNotTraced()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person);

			Target.ActivityCheck(person);

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
			Target.Trace(userCode2);

			Target.ActivityCheck(person);

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
			Target.Trace("usercode2");

			Target.ActivityCheck(person1);
			Target.ActivityCheck(person2);

			Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Contain("usercode2");
			Logs.ReadOfType<StateTraceLog>().Single().Log.User.Should().Not.Contain("usercode1");
		}

		[Test]
		public void ShouldLogMessage()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person);
			Target.Trace("usercode");

			Target.ActivityCheck(person);

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
			Target.Trace("usercode");

			Target.ActivityCheck(person1);

			var actual = Logs.ReadOfType<TracingLog>().Single();
			actual.Log.Tracing.Should().Contain("usercode");
			actual.Log.Tracing.Should().Contain("jågej");
			actual.Log.Tracing.Should().Contain("kjax");
			actual.Log.Tracing.Should().Contain("pierre");
			actual.Log.Tracing.Should().Contain("baldi");
		}
	}
}