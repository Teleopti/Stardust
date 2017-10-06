using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	public class RtaTracerViewModelBuilderTest
	{
		public RtaTracerViewModelBuilder Target;
		public FakeTraceReader Traces;

		[Test]
		public void ShouldBuildSomething()
		{
			Target.Build().Should().Be.OfType<RtaTracerViewModel>();
		}

		[Test]
		public void ShouldContainTracer()
		{
			Traces.Has(new RtaTracerLog<DataRecievedAtLog> {Process = "process"});

			Target.Build().Tracers.Single().Process.Should().Be("process");
		}

		[Test]
		public void ShouldContainTracers()
		{
			Traces
				.Has(new RtaTracerLog<DataRecievedAtLog> {Process = "process1"})
				.Has(new RtaTracerLog<DataRecievedAtLog> {Process = "process2"});

			Target.Build()
				.Tracers.Select(x => x.Process)
				.Should().Have.SameValuesAs("process1", "process2");
		}

		[Test]
		public void ShouldMapTracersSyncronously()
		{
			var process = new RtaTracerLog<DataRecievedAtLog> {Process = "process"};
			Traces.Has(process);

			var result = Target.Build();
			process.Process = "mutated";

			result.Tracers.Single().Process.Should().Be("process");
		}

		[Test]
		public void ShouldContainTracing()
		{
			Traces
				.Has(new RtaTracerLog<TracingLog>
				{
					Log = new TracingLog
					{
						Tracing = "userCode"
					}
				});

			Target.Build().Tracers.Single().Tracing.Should().Be("userCode");
		}

		[Test]
		public void ShouldOnlyContainLatestTracing()
		{
			Traces
				.Has(new RtaTracerLog<TracingLog>
				{
					Process = "process",
					Time = "2017-10-05 10:01".Utc(),
					Log = new TracingLog
					{
						Tracing = "userCode1"
					}
				})
				.Has(new RtaTracerLog<TracingLog>
				{
					Process = "process",
					Time = "2017-10-05 10:00".Utc(),
					Log = new TracingLog
					{
						Tracing = "userCode2"
					}
				});

			Target.Build().Tracers.Single().Tracing.Should().Be("userCode1");
		}

		[Test]
		public void ShouldContainTracingForTheProcess()
		{
			Traces
				.Has(new RtaTracerLog<TracingLog>
				{
					Process = "process1",
					Log = new TracingLog
					{
						Tracing = "userCode1"
					}
				})
				.Has(new RtaTracerLog<TracingLog>
				{
					Process = "process2",
					Log = new TracingLog
					{
						Tracing = "userCode2"
					}
				});

			Target.Build().Tracers.Single(x => x.Process == "process1").Tracing.Should().Be("userCode1");
		}

		[Test]
		public void ShouldContainDataRecievedAt()
		{
			Traces
				.Has(new RtaTracerLog<DataRecievedAtLog>
				{
					Log = new DataRecievedAtLog {DataRecievedAt = "2017-10-04 08:00:01".Utc()}
				});

			Target.Build().Tracers.Single().DataReceivedAt.Should().Be("08:00:01");
		}

		[Test]
		public void ShouldOnlyContainLatestDataRecievedAt()
		{
			Traces
				.Has(new RtaTracerLog<DataRecievedAtLog>
				{
					Process = "process",
					Log = new DataRecievedAtLog
					{
						DataRecievedAt = "2017-10-04 08:00:01".Utc()
					}
				})
				.Has(new RtaTracerLog<DataRecievedAtLog>
				{
					Process = "process",
					Log = new DataRecievedAtLog
					{
						DataRecievedAt = "2017-10-04 08:00:11".Utc()
					}
				});

			Target.Build().Tracers.Single().DataReceivedAt.Should().Be("08:00:11");
		}

		[Test]
		public void ShouldContainDataReceivedAtForTheProcess()
		{
			Traces
				.Has(new RtaTracerLog<DataRecievedAtLog>
				{
					Process = "p1",
					Log = new DataRecievedAtLog
					{
						DataRecievedAt = "2017-10-05 08:00:00".Utc()
					}
				})
				.Has(new RtaTracerLog<DataRecievedAtLog>
				{
					Process = "p2",
					Log = new DataRecievedAtLog
					{
						DataRecievedAt = "2017-10-05 08:00:10".Utc()
					}
				});

			Target.Build().Tracers.Single(x => x.Process == "p1").DataReceivedAt
				.Should().Be("08:00:00");
		}

		[Test]
		public void ShouldContainAcitivtyCheckAt()
		{
			Traces
				.Has(new RtaTracerLog<ActivityCheckAtLog>
				{
					Log = new ActivityCheckAtLog
					{
						ActivityCheckAt = "2017-10-04 08:00:01".Utc()
					}
				});

			Target.Build().Tracers.Single().ActivityCheckAt.Should().Be("08:00:01");
		}

		[Test]
		public void ShouldContainActivityCheckAtForTheProcess()
		{
			Traces
				.Has(new RtaTracerLog<ActivityCheckAtLog>
				{
					Process = "p1",
					Log = new ActivityCheckAtLog
					{
						ActivityCheckAt = "2017-10-05 08:00:00".Utc()
					}
				})
				.Has(new RtaTracerLog<ActivityCheckAtLog>
				{
					Process = "p2",
					Log = new ActivityCheckAtLog
					{
						ActivityCheckAt = "2017-10-05 08:00:10".Utc()
					}
				});

			Target.Build().Tracers.Single(x => x.Process == "p1").ActivityCheckAt
				.Should().Be("08:00:00");
		}

		[Test]
		public void ShouldContainLatestActivityCheckedAt()
		{
			Traces
				.Has(new RtaTracerLog<ActivityCheckAtLog>
				{
					Process = "process",
					Log = new ActivityCheckAtLog
					{
						ActivityCheckAt = "2017-10-05 08:00:00".Utc()
					}
				})
				.Has(new RtaTracerLog<ActivityCheckAtLog>
				{
					Process = "process",
					Log = new ActivityCheckAtLog
					{
						ActivityCheckAt = "2017-10-05 08:00:10".Utc()
					}
				});

			Target.Build().Tracers.Single().ActivityCheckAt
				.Should().Be("08:00:10");
		}

		[Test]
		public void ShouldHaveTracedUser()
		{
			Traces
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						User = "usercode"
					}
				})
				;

			Target.Build().TracedUsers.Single().User
				.Should().Be("usercode");
		}

		[Test]
		public void ShouldHave2TracedUsers()
		{
			Traces
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						User = "usercode1"
					}
				}, new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						User = "usercode2"
					}
				})
				;

			Target.Build().TracedUsers.Select(x => x.User)
				.Should().Have.SameValuesAs("usercode1", "usercode2");
		}

		[Test]
		public void ShouldHaveStateCode()
		{
			Traces
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						StateCode = "statecode"
					}
				})
				;

			Target.Build().TracedUsers.Single().States.Single().StateCode
				.Should().Be("statecode");
		}

		[Test]
		public void ShouldHave2StateCodes()
		{
			Traces
				.Has(new RtaTracerLog<StateTraceLog>
					{
						Log = new StateTraceLog
						{
							Id = Guid.NewGuid(),
							StateCode = "statecode1"
						}
					},
					new RtaTracerLog<StateTraceLog>
					{
						Log = new StateTraceLog
						{
							Id = Guid.NewGuid(),
							StateCode = "statecode2"
						}
					})
				;

			Target.Build().TracedUsers.Single().States.Select(x => x.StateCode)
				.Should().Have.SameValuesAs("statecode1", "statecode2");
		}

		[Test]
		public void ShouldHaveMessage()
		{
			var traceId = Guid.NewGuid();
			Traces
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						Id = traceId,
						StateCode = "statecode"
					}
				})
				.Has(
					new RtaTracerLog<StateTraceLog>
					{
						Log = new StateTraceLog
						{
							Id = traceId
						},
						Message = "Processing"
					})
				;

			Target.Build().TracedUsers.Single().States.Single().Traces.Single()
				.Should().Be("Processing");
		}

		[Test]
		public void ShouldHaveMessageToo()
		{
			Traces
				.Has(
					new RtaTracerLog<StateTraceLog>
					{
						Log = new StateTraceLog
						{
							Id = Guid.NewGuid()
						},
						Message = "Processing"
					})
				;

			Target.Build().TracedUsers.Single().States.Single().Traces.Single()
				.Should().Be("Processing");
		}

		[Test]
		public void ShouldHaveMessageWithProcess()
		{
			Traces
				.Has(
					new RtaTracerLog<StateTraceLog>
					{
						Process = "process",
						Log = new StateTraceLog
						{
							Id = Guid.NewGuid()
						},
						Message = "message"
					})
				;

			Target.Build().TracedUsers.Single().States.Single().Traces.Single()
				.Should().Contain("process");
		}

		[Test]
		public void ShouldHave2Messages()
		{
			var traceId = Guid.NewGuid();
			Traces
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						Id = traceId,
						StateCode = "statecode"
					}
				})
				.Has(new RtaTracerLog<StateTraceLog>
					{
						Log = new StateTraceLog
						{
							Id = traceId,
						},
						Message = "Processing"
					},
					new RtaTracerLog<StateTraceLog>
					{
						Log = new StateTraceLog
						{
							Id = traceId,
						},
						Message = "Processed"
					})
				;

			Target.Build().TracedUsers.Single().States.Single().Traces
				.Should().Have.SameValuesAs("Processing", "Processed");
		}

		[Test]
		public void ShouldHaveMessageForEachTrace()
		{
			var trace1 = Guid.NewGuid();
			var trace2 = Guid.NewGuid();
			Traces
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						Id = trace1,
						StateCode = "statecode"
					}
				})
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						Id = trace1,
						StateCode = "statecode"
					},
					Message = "Processing"
				})
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Log = new StateTraceLog
					{
						Id = trace2,
						StateCode = "statecode"
					},
					Message = "Processing"
				})
				;

			Target.Build().TracedUsers.Single().States.Select(x => x.Traces.Single())
				.Should().Have.SameSequenceAs("Processing", "Processing");
		}

		[Test]
		public void ShouldMapTracedUsersSyncronously()
		{
			var recevied = new RtaTracerLog<StateTraceLog> {Log = new StateTraceLog {User = "usercode", StateCode = "statecode"}, Message = "message"};
			Traces.Has(recevied);

			var result = Target.Build();
			recevied.Log.User = "mutated";
			recevied.Log.StateCode = "mutated";
			recevied.Message = "mutated";
			
			result.TracedUsers.Single().User
				.Should().Be("usercode");
			result.TracedUsers.Single().States.Single().StateCode
				.Should().Be("statecode");
			result.TracedUsers.Single().States.Single().Traces.Single()
				.Should().Be("message");
		}
		
	}
}