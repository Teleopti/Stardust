using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;
using Teleopti.Wfm.Adherence.Test.States.Unit.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service
{
	[TestFixture]
	[DatabaseTest]
	[ExtendScope(typeof(PersonAssociationChangedEventPublisher))]
	[ExtendScope(typeof(AgentStateMaintainer))]
	[ExtendScope(typeof(AgentStateReadModelMaintainer))]
	[ExtendScope(typeof(AgentStateReadModelUpdater))]
	[ExtendScope(typeof(MappingReadModelUpdater))]
	[ExtendScope(typeof(CurrentScheduleReadModelUpdater))]
	[ExtendScope(typeof(ExternalLogonReadModelUpdater))]
	public class UnrecognizedStatesTest2
	{
		public ILogOnOffContext Context;
		public Rta Target;
		public Database Database;
		public AnalyticsDatabase Analytics;
		public WithUnitOfWork UnitOfWork;
		public IRtaStateGroupRepository StateGroupRepository;

		[Test]
		public void ShouldNotAddDuplicatesInBatch()
		{
			Analytics.WithDataSource(7, new BatchForTest().SourceId);
			Database
				.WithAgent("usercode1")
				.WithAgent("usercode2")
				.WithAgent("usercode3")
				.WithStateGroup("default", true)
				.WithStateCode("default")
				.WithStateGroup("logged out", false, true)
				.WithStateCode("LOGGED-OFF")
				.PublishRecurringEvents();
			Context.Logout();

			Target.Process(new BatchForTest
			{
				SnapshotId = "2016-05-18 08:00".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode1",
						StateCode = "phone"
					},
					new BatchStateForTest
					{
						UserCode = "usercode2",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-05-18 08:00".Utc()
			});

			Target.Process(new BatchForTest
			{
				SnapshotId = "2016-05-18 08:05".Utc(),
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode3",
						StateCode = "phone"
					}
				}
			});
			Target.CloseSnapshot(new CloseSnapshotForTest
			{
				SnapshotId = "2016-05-18 08:05".Utc()
			});

			Context.Login();
			var actual = UnitOfWork.Get(() => StateGroupRepository.LoadAllCompleteGraph());
			actual.SelectMany(g => g.StateCollection.Select(s => s.StateCode))
				.Where(x => x == Rta.LogOutBySnapshot)
				.Should().Have.Count.EqualTo(1);
		}
	}
}