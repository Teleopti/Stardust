using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service
{
	[TestFixture]
	[DatabaseTest]
	public class QueueueueueeTest : IIsolateSystem
	{
		public Rta Target;
		public Database Database;
		public AnalyticsDatabase Analytics;
		public IAgentStateReadModelPersister AgentStates;
		public IRtaStateGroupRepository StateGroups;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;

		public FakeEventPublisher Publisher;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldAddMissingStateCodeToDefaultStateGroup()
		{
			Now.Is("2015-05-13 08:00");
			Publisher.AddHandler(typeof(PersonAssociationChangedEventPublisher));
			Publisher.AddHandler(typeof(AgentStateMaintainer));
			Publisher.AddHandler(typeof(MappingReadModelUpdater));
			Publisher.AddHandler(typeof(ExternalLogonReadModelUpdater));
			Publisher.AddHandler(typeof(AgentStateReadModelMaintainer));
			Publisher.AddHandler(typeof(AgentStateReadModelUpdater));
			Analytics.WithDataSource(7, new BatchForTest().SourceId);
			Database
				.WithAgent("user")
				.WithStateGroup("phone")
				.WithStateCode("phone")
				.PublishRecurringEvents();
			var personId = Database.CurrentPersonId();
			var stateGroupId = WithUnitOfWork.Get(() => StateGroups.LoadAll()).Single().Id.Value;
			
			Target.Enqueue(new BatchForTest
			{
				States = new[]
				{
					new BatchStateInputModel
					{
						UserCode = "user",
						StateCode = "phone"
					}
				}
			});
			
			Target.QueueIteration(InfraTestConfigReader.TenantName());

			WithUnitOfWork.Get(() => AgentStates.Load(personId)).StateGroupId.Should().Be(stateGroupId);
		}
	}
}