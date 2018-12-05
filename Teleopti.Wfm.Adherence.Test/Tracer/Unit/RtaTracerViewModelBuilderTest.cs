using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.Test.Tracer.Unit
{
	[DomainTest]
	[Setting("UseSafeRtaTracer", false)]
	public class RtaTracerViewModelBuilderTest
	{
		public RtaTracerViewModelBuilder Target;
		public FakeRtaTracerPersister RtaTracers;
		public ICurrentDataSource DataSource;

		[Test]
		public void ShouldBuildSomething()
		{
			Target.Build().Should().Be.OfType<RtaTracerViewModel>();
		}

		[Test]
		public void ShouldContainTracer()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessReceivedLog> {Process = "process"});

			Target.Build().Tracers.Single().Process.Should().Be("process");
		}

		[Test]
		public void ShouldContainTracers()
		{
			RtaTracers
				.WithCurrentTenant()
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
			RtaTracers
				.WithCurrentTenant()
				.Has(process);

			var result = Target.Build();
			process.Process = "mutated";

			result.Tracers.Single().Process.Should().Be("process");
		}

		[Test]
		public void ShouldContainTracing()
		{
			RtaTracers
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
		[SetCulture("sv-SE")]
		public void ShouldContainDataReceivedAt()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Log = new ProcessReceivedLog {At = "2017-10-04 08:00:01.123".Utc()}
				});

			Target.Build().Tracers.Single().DataReceived.Single().At.Should().Be("08:00:01");
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldContainDataReceivedAtInUSA()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Log = new ProcessReceivedLog {At = "2017-10-04 13:00:01".Utc()}
				});

			Target.Build().Tracers.Single().DataReceived.Single().At.Should().Be("13:00:01");
		}

		[Test]
		public void ShouldContainLatestDataReceivedFirst()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Process = "process",
					Log = new ProcessReceivedLog
					{
						At = "2017-10-04 08:00:01".Utc()
					}
				})
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Process = "process",
					Log = new ProcessReceivedLog
					{
						At = "2017-10-04 08:00:11".Utc()
					}
				});

			Target.Build().Tracers.Single().DataReceived.First().At.Should().Be("08:00:11");
		}

		[Test]
		public void ShouldContain5LatestDataReceivals()
		{
			var times = "2017-12-19 12:00".Utc().TimeRange("2017-12-19 13:00".Utc(), TimeSpan.FromMinutes(1));
			var logs = times.Select(x => new RtaTracerLog<ProcessReceivedLog>
			{
				Log = new ProcessReceivedLog {At = x}
			});
			RtaTracers
				.WithCurrentTenant()
				.Has(logs.ToArray());

			Target.Build().Tracers.Single().DataReceived.Should().Have.Count.EqualTo(5);
			Target.Build().Tracers.Single().DataReceived.ElementAt(0).At.Should().Be("13:00:00");
			Target.Build().Tracers.Single().DataReceived.ElementAt(1).At.Should().Be("12:59:00");
			Target.Build().Tracers.Single().DataReceived.ElementAt(2).At.Should().Be("12:58:00");
			Target.Build().Tracers.Single().DataReceived.ElementAt(3).At.Should().Be("12:57:00");
			Target.Build().Tracers.Single().DataReceived.ElementAt(4).At.Should().Be("12:56:00");
		}

		[Test]
		public void ShouldContainDataReceivedAtForTheProcess()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Process = "p1",
					Log = new ProcessReceivedLog
					{
						At = "2017-10-05 08:00:00".Utc()
					}
				})
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Process = "p2",
					Log = new ProcessReceivedLog
					{
						At = "2017-10-05 08:00:10".Utc()
					}
				});

			Target.Build().Tracers.Single(x => x.Process == "p1").DataReceived.Single().At
				.Should().Be("08:00:00");
		}

		[Test]
		public void ShouldContainDataReceivedByAndCount()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessReceivedLog>
				{
					Log = new ProcessReceivedLog
					{
						By = "method",
						Count = 123
					}
				});

			Target.Build().Tracers.Single().DataReceived.Single().By.Should().Be("method");
			Target.Build().Tracers.Single().DataReceived.Single().Count.Should().Be(123);
		}

		[Test]
		[SetCulture("sv-SE")]
		public void ShouldContainActivtyCheckAt()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessActivityCheckLog>
				{
					Log = new ProcessActivityCheckLog
					{
						At = "2017-10-04 08:00:01.123".Utc()
					}
				});

			Target.Build().Tracers.Single().ActivityCheck.Single().At.Should().Be("08:00:01");
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldContainActivtyCheckAtInUSA()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessActivityCheckLog>
				{
					Log = new ProcessActivityCheckLog
					{
						At = "2017-10-04 08:00:01".Utc()
					}
				});

			Target.Build().Tracers.Single().ActivityCheck.Single().At.Should().Be("08:00:01");
		}

		[Test]
		[SetCulture("sv-SE")]
		public void ShouldContainActivityCheckAtForTheProcess()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessActivityCheckLog>
				{
					Process = "p1",
					Log = new ProcessActivityCheckLog
					{
						At = "2017-10-05 08:00:00".Utc()
					}
				})
				.Has(new RtaTracerLog<ProcessActivityCheckLog>
				{
					Process = "p2",
					Log = new ProcessActivityCheckLog
					{
						At = "2017-10-05 08:00:10".Utc()
					}
				});

			Target.Build().Tracers.Single(x => x.Process == "p1").ActivityCheck.Single().At
				.Should().Be("08:00:00");
		}


		[Test]
		public void ShouldContain5LatestActivityChecks()
		{
			var times = "2017-12-19 12:00".Utc().TimeRange("2017-12-19 13:00".Utc(), TimeSpan.FromMinutes(1));
			var logs = times.Select(x => new RtaTracerLog<ProcessActivityCheckLog>
			{
				Log = new ProcessActivityCheckLog {At = x}
			});
			RtaTracers
				.WithCurrentTenant()
				.Has(logs.ToArray());

			Target.Build().Tracers.Single().ActivityCheck.Should().Have.Count.EqualTo(5);
			Target.Build().Tracers.Single().ActivityCheck.ElementAt(0).At.Should().Be("13:00:00");
			Target.Build().Tracers.Single().ActivityCheck.ElementAt(1).At.Should().Be("12:59:00");
			Target.Build().Tracers.Single().ActivityCheck.ElementAt(2).At.Should().Be("12:58:00");
			Target.Build().Tracers.Single().ActivityCheck.ElementAt(3).At.Should().Be("12:57:00");
			Target.Build().Tracers.Single().ActivityCheck.ElementAt(4).At.Should().Be("12:56:00");
		}

		[Test]
		public void ShouldContainProcessException()
		{
			var exceptionType = RandomName.Make(nameof(InvalidAuthenticationKeyException));
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessExceptionLog>
				{
					Time = "2018-01-09 12:00".Utc(),
					Log = new ProcessExceptionLog
					{
						Type = exceptionType,
						Info = "Alot of info"
					}
				});

			Target.Build().Tracers.Single().Exceptions.Single().Exception.Should().Be(exceptionType);
			Target.Build().Tracers.Single().Exceptions.Single().At.Should().Be("2018-01-09 12:00:00");
			Target.Build().Tracers.Single().Exceptions.Single().Info.Should().Be("Alot of info");
		}

		[Test]
		public void ShouldContain5LatestProcessExceptions()
		{
			var times = "2017-12-19 12:00".Utc().TimeRange("2017-12-19 13:00".Utc(), TimeSpan.FromMinutes(1));
			var logs = times.Select(x => new RtaTracerLog<ProcessExceptionLog>
			{
				Time = x,
				Log = new ProcessExceptionLog()
			});
			RtaTracers
				.WithCurrentTenant()
				.Has(logs.ToArray());

			Target.Build().Tracers.Single().Exceptions.Should().Have.Count.EqualTo(5);
			Target.Build().Tracers.Single().Exceptions.ElementAt(0).At.Should().Be("2017-12-19 13:00:00");
			Target.Build().Tracers.Single().Exceptions.ElementAt(1).At.Should().Be("2017-12-19 12:59:00");
			Target.Build().Tracers.Single().Exceptions.ElementAt(2).At.Should().Be("2017-12-19 12:58:00");
			Target.Build().Tracers.Single().Exceptions.ElementAt(3).At.Should().Be("2017-12-19 12:57:00");
			Target.Build().Tracers.Single().Exceptions.ElementAt(4).At.Should().Be("2017-12-19 12:56:00");
		}

		[Test]
		public void ShouldHaveTracedUser()
		{
			RtaTracers
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
				.WithCurrentTenant()
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
			RtaTracers
				.WithCurrentTenant()
				.Has(recevied);

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

		[Test]
		[SetCulture("sv-SE")]
		public void ShouldContainDataEnqueuing()
		{
			RtaTracers
				.WithCurrentTenant()
				.Has(new RtaTracerLog<ProcessEnqueuingLog>
				{
					Log = new ProcessEnqueuingLog {At = "2017-10-04 08:00:01.123".Utc(), Count = 3}
				});

			Target.Build().Tracers.Single().DataEnqueuing.Single().At.Should().Be("08:00:01");
			Target.Build().Tracers.Single().DataEnqueuing.Single().Count.Should().Be(3);
		}

		[Test]
		public void ShouldContain5LatestDataEnqueuings()
		{
			var times = "2017-12-19 12:00".Utc().TimeRange("2017-12-19 13:00".Utc(), TimeSpan.FromMinutes(1));
			var logs = times.Select(x => new RtaTracerLog<ProcessEnqueuingLog>
			{
				Log = new ProcessEnqueuingLog {At = x}
			});
			RtaTracers
				.WithCurrentTenant()
				.Has(logs.ToArray());

			Target.Build().Tracers.Single().DataEnqueuing.Should().Have.Count.EqualTo(5);
			Target.Build().Tracers.Single().DataEnqueuing.ElementAt(0).At.Should().Be("13:00:00");
			Target.Build().Tracers.Single().DataEnqueuing.ElementAt(1).At.Should().Be("12:59:00");
			Target.Build().Tracers.Single().DataEnqueuing.ElementAt(2).At.Should().Be("12:58:00");
			Target.Build().Tracers.Single().DataEnqueuing.ElementAt(3).At.Should().Be("12:57:00");
			Target.Build().Tracers.Single().DataEnqueuing.ElementAt(4).At.Should().Be("12:56:00");
		}

		[Test]
		[SetCulture("sv-SE")]
		public void ShouldContainDataProcessing()
		{
			RtaTracers
				.Has(new RtaTracerLog<ProcessProcessingLog>
				{
					Log = new ProcessProcessingLog {At = "2017-10-04 08:00:01.123".Utc(), Count = 3}
				});

			Target.Build().Tracers.Single().DataProcessing.Single().At.Should().Be("08:00:01");
			Target.Build().Tracers.Single().DataProcessing.Single().Count.Should().Be(3);
		}

		[Test]
		public void ShouldContain5LatestDataProcessings()
		{
			var times = "2017-12-19 12:00".Utc().TimeRange("2017-12-19 13:00".Utc(), TimeSpan.FromMinutes(1));
			var logs = times.Select(x => new RtaTracerLog<ProcessProcessingLog>
			{
				Log = new ProcessProcessingLog {At = x}
			});
			RtaTracers
				.WithCurrentTenant()
				.Has(logs.ToArray());

			Target.Build().Tracers.Single().DataProcessing.Should().Have.Count.EqualTo(5);
			Target.Build().Tracers.Single().DataProcessing.ElementAt(0).At.Should().Be("13:00:00");
			Target.Build().Tracers.Single().DataProcessing.ElementAt(1).At.Should().Be("12:59:00");
			Target.Build().Tracers.Single().DataProcessing.ElementAt(2).At.Should().Be("12:58:00");
			Target.Build().Tracers.Single().DataProcessing.ElementAt(3).At.Should().Be("12:57:00");
			Target.Build().Tracers.Single().DataProcessing.ElementAt(4).At.Should().Be("12:56:00");
		}
	}
}