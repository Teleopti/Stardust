using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Tracer
{
	[DomainTest]
	public class RtaTracerViewModelBuilderTest
	{
		public RtaTracerViewModelBuilder Target;
		public FakeRtaTracerPersister RtaTracers;

		[Test]
		public void ShouldBuildSomething()
		{
			Target.Build().Should().Be.OfType<RtaTracerViewModel>();
		}

		[Test]
		public void ShouldContainTracer()
		{
			RtaTracers.Has(new RtaTracerLog<ProcessReceivedLog> {Process = "process"});

			Target.Build().Tracers.Single().Process.Should().Be("process");
		}

		[Test]
		public void ShouldContainTracers()
		{
			RtaTracers
				.Has(new RtaTracerLog<ProcessReceivedLog> {Process = "process1"})
				.Has(new RtaTracerLog<ProcessReceivedLog> {Process = "process2"});

			Target.Build()
				.Tracers.Select(x => x.Process)
				.Should().Have.SameValuesAs("process1", "process2");
		}

		[Test]
		public void ShouldMapTracersSyncronously()
		{
			var process = new RtaTracerLog<ProcessReceivedLog> {Process = "process"};
			RtaTracers.Has(process);

			var result = Target.Build();
			process.Process = "mutated";

			result.Tracers.Single().Process.Should().Be("process");
		}

		[Test]
		public void ShouldContainTracing()
		{
			RtaTracers
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
			RtaTracers
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
			RtaTracers
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
			RtaTracers
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Log = new ProcessReceivedLog {RecievedAt = "2017-10-04 08:00:01".Utc()}
				});

			Target.Build().Tracers.Single().DataReceivedAt.Should().Be("08:00:01");
		}

		[Test]
		public void ShouldOnlyContainLatestDataRecievedAt()
		{
			RtaTracers
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Process = "process",
					Log = new ProcessReceivedLog
					{
						RecievedAt = "2017-10-04 08:00:01".Utc()
					}
				})
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Process = "process",
					Log = new ProcessReceivedLog
					{
						RecievedAt = "2017-10-04 08:00:11".Utc()
					}
				});

			Target.Build().Tracers.Single().DataReceivedAt.Should().Be("08:00:11");
		}

		[Test]
		public void ShouldContainDataReceivedAtForTheProcess()
		{
			RtaTracers
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Process = "p1",
					Log = new ProcessReceivedLog
					{
						RecievedAt = "2017-10-05 08:00:00".Utc()
					}
				})
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Process = "p2",
					Log = new ProcessReceivedLog
					{
						RecievedAt = "2017-10-05 08:00:10".Utc()
					}
				});

			Target.Build().Tracers.Single(x => x.Process == "p1").DataReceivedAt
				.Should().Be("08:00:00");
		}

		[Test]
		public void ShouldContainAcitivtyCheckAt()
		{
			RtaTracers
				.Has(new RtaTracerLog<ActivityCheckLog>
				{
					Log = new ActivityCheckLog
					{
						ActivityCheckAt = "2017-10-04 08:00:01".Utc()
					}
				});

			Target.Build().Tracers.Single().ActivityCheckAt.Should().Be("08:00:01");
		}

		[Test]
		public void ShouldContainActivityCheckAtForTheProcess()
		{
			RtaTracers
				.Has(new RtaTracerLog<ActivityCheckLog>
				{
					Process = "p1",
					Log = new ActivityCheckLog
					{
						ActivityCheckAt = "2017-10-05 08:00:00".Utc()
					}
				})
				.Has(new RtaTracerLog<ActivityCheckLog>
				{
					Process = "p2",
					Log = new ActivityCheckLog
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
			RtaTracers
				.Has(new RtaTracerLog<ActivityCheckLog>
				{
					Process = "process",
					Log = new ActivityCheckLog
					{
						ActivityCheckAt = "2017-10-05 08:00:00".Utc()
					}
				})
				.Has(new RtaTracerLog<ActivityCheckLog>
				{
					Process = "process",
					Log = new ActivityCheckLog
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
			RtaTracers
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
			RtaTracers
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
		public void ShouldHave2TracedUsersOrderedByLatestFirst()
		{
			RtaTracers
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Time = "2017-10-18 10:00".Utc(),
					Log = new StateTraceLog
					{
						User = "usercode1"
					}
				})
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Time = "2017-10-18 10:01".Utc(),
					Log = new StateTraceLog
					{
						User = "usercode2"
					}
				})
				;

			Target.Build().TracedUsers.Select(x => x.User)
				.Should().Have.SameSequenceAs("usercode2", "usercode1");
		}

		[Test]
		public void ShouldHaveStateCode()
		{
			RtaTracers
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
			RtaTracers
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
		public void ShouldHave2StateCodesOrderedByLatestFirst()
		{
			RtaTracers
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Time = "2017-10-18 10:00".Utc(),
					Log = new StateTraceLog
					{
						User = "usercode",
						Id = Guid.NewGuid(),
						StateCode = "statecode1"
					}
				})
				.Has(new RtaTracerLog<StateTraceLog>
				{
					Time = "2017-10-18 10:01".Utc(),
					Log = new StateTraceLog
					{
						User = "usercode",
						Id = Guid.NewGuid(),
						StateCode = "statecode2"
					}
				})
				;

			Target.Build().TracedUsers.Single().States.Select(x => x.StateCode)
				.Should().Have.SameSequenceAs("statecode2", "statecode1");
		}

		[Test]
		public void ShouldHaveMessage()
		{
			var traceId = Guid.NewGuid();
			RtaTracers
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
			RtaTracers
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
			RtaTracers
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
			RtaTracers
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
		public void ShouldHave2MessagesOrderedByLatestLast()
		{
			var traceId = Guid.NewGuid();
			RtaTracers
				.Has(new RtaTracerLog<StateTraceLog>
					{
						Time = "2017-10-18 10:00".Utc(),
						Log = new StateTraceLog
						{
							Id = traceId,
						},
						Message = "Processing"
					},
					new RtaTracerLog<StateTraceLog>
					{
						Time = "2017-10-18 10:01".Utc(),
						Log = new StateTraceLog
						{
							Id = traceId,
						},
						Message = "Processed"
					})
				;

			Target.Build().TracedUsers.Single().States.Single().Traces
				.Should().Have.SameSequenceAs("Processing", "Processed");
		}

		[Test]
		public void ShouldHaveMessageForEachTrace()
		{
			var trace1 = Guid.NewGuid();
			var trace2 = Guid.NewGuid();
			RtaTracers
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
			var recevied = new RtaTracerLog<StateTraceLog>
			{
				Log = new StateTraceLog {User = "usercode", StateCode = "statecode"},
				Message = "message"
			};
			RtaTracers.Has(recevied);

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