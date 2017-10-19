using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Service
{
	[TestFixture]
	[Toggle(Toggles.RTA_RtaTracer_45597)]
	[PrincipalAndStateTest]
	[Setting("RtaTracerBufferSize", 0)]
	public class TracingTest : ISetup
	{
		public Database Database;
		public AnalyticsDatabase Analytics;
		public Domain.ApplicationLayer.Rta.Service.Rta Rta;
		public IRtaTracer Tracer;
		public FakeEventPublisher Publisher;
		public IRtaTracerReader Reader;
		public IPrincipalAndStateContext Context;
		public ICurrentDataSource DataSource;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
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
		
			Tracer.ProcessReceived();
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