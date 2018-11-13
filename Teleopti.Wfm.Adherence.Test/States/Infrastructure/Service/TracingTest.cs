using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service
{
	[TestFixture]
	[DatabaseTest]
	[Setting("RtaTracerBufferSize", 0)]
	public class TracingTest : IIsolateSystem
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public Rta Rta;
		public IRtaTracer Tracer;
		public FakeEventPublisher Publisher;
		public IRtaTracerReader Reader;
		public ILogOnOffContext Context;
		public ICurrentDataSource DataSource;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldTraceState()
		{
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			Publisher.AddHandler(typeof(CurrentScheduleReadModelUpdater));
			Publisher.AddHandler(typeof(ExternalLogonReadModelUpdater));
			Publisher.AddHandler(typeof(AgentStateReadModelUpdater));
			Analytics.WithDataSource(9, "sourceId");
			Database
				.WithAgent("usercode1")
				.WithStateGroup("phone")
				.WithStateCode("phone")
				.PublishRecurringEvents()
				;
			Tracer.Trace("usercode1");
			Context.Logout();
		
			Tracer.ProcessReceived(null, null);
			Rta.Process(new BatchForTest
			{
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "phone"
					}
				}
			});

			Context.Login();
			Reader.ReadOfType<ProcessReceivedLog>().Should().Not.Be.Empty();
			Reader.ReadOfType<StateTraceLog>().Should().Not.Be.Empty();
		}
	}
}