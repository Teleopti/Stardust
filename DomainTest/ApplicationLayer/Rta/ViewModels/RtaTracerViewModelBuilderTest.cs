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
		public void ShouldMapSyncronously()
		{
			var process = new RtaTracerLog<DataRecievedAtLog> {Process = "process"};
			Traces.Has(process);

			var result = Target.Build();
			process.Process = "mutated";

			result.Tracers.Single().Process.Should().Be("process");
		}

		[Test]
		public void ShouldContainDataRecievedAt()
		{
			Traces
				.Has(new RtaTracerLog<DataRecievedAtLog>
				{
					Thing = new DataRecievedAtLog {DataRecievedAt = "2017-10-04 08:00:01".Utc()}
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
					Thing = new DataRecievedAtLog
					{
						DataRecievedAt = "2017-10-04 08:00:01".Utc()
					}
				})
				.Has(new RtaTracerLog<DataRecievedAtLog>
				{
					Process = "process",
					Thing = new DataRecievedAtLog
					{
						DataRecievedAt = "2017-10-04 08:00:11".Utc()
					}
				});

			Target.Build().Tracers.Single().DataReceivedAt.Should().Be("08:00:11");
		}

		[Test]
		public void ShouldContainAcitivtyCheckedAt()
		{
			Traces
				.Has(new RtaTracerLog<ActivityCheckAtLog>
				{
					Thing = new ActivityCheckAtLog
					{
						ActivityCheckAt = "2017-10-04 08:00:01".Utc()
					}
				});
			
			Target.Build().Tracers.Single().ActivityCheckAt.Should().Be("08:00:01");
		}

		[Test]
		public void ShouldContainTracing()
		{
			Traces
				.Has(new RtaTracerLog<TracingLog>
				{
					Thing = new TracingLog
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
					Thing = new TracingLog
					{
						Tracing = "userCode1"
					}
				})
				.Has(new RtaTracerLog<TracingLog>
				{
					Process = "process",
					Thing = new TracingLog
					{
						Tracing = "userCode2"
					}
				});
			
			Target.Build().Tracers.Single().Tracing.Should().Be("userCode2");
		}
	}
}