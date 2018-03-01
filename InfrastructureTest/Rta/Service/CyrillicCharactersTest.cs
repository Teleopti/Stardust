using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Rta.Service;
using Teleopti.Ccc.Domain.Rta.ViewModels;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Service
{
	[TestFixture]
	[PrincipalAndStateTest]
	[ExtendScope(typeof(PersonAssociationChangedEventPublisher))]
	[ExtendScope(typeof(AgentStateMaintainer))]
	[ExtendScope(typeof(AgentStateReadModelMaintainer))]
	[ExtendScope(typeof(AgentStateReadModelUpdater))]
	[ExtendScope(typeof(MappingReadModelUpdater))]
	[ExtendScope(typeof(CurrentScheduleReadModelUpdater))]
	[ExtendScope(typeof(ExternalLogonReadModelUpdater))]
	public class CyrillicCharactersTest
	{
		public IPrincipalAndStateContext Context;
		public Domain.Rta.Service.Rta Target;
		public Database Database;
		public AnalyticsDatabase Analytics;
		public IAgentStateReadModelReader AgentStateReadModel;
		public IRtaStateGroupRepository StateGroupRepository;
		public WithUnitOfWork UnitOfWork;

		[Test]
		public void ShouldMatchWithCyrillicCharacters()
		{
			Analytics.WithDataSource(7, new BatchForTest().SourceId);
			Database
				.WithAgent("usercode")
				.WithStateGroup("default", true)
				.WithStateGroup("Телефон")
				.WithStateCode("Телефон")
				.PublishRecurringEvents();
			Context.Logout();

			Target.Process(new BatchForTest
			{
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode",
						StateCode = "Телефон"
					}
				}
			});

			Context.Login();
			var actual = UnitOfWork.Get(() => AgentStateReadModel.Read(new AgentStateFilter()).Single());
			actual.StateName.Should().Be("Телефон");
		}

		[Test]
		public void ShouldAddUnknownStateCodeWithCyrillicCharacters()
		{
			Analytics.WithDataSource(7, new BatchForTest().SourceId);
			Database
				.WithAgent("usercode")
				.WithStateGroup("default", true)
				.WithStateCode("default")
				.PublishRecurringEvents();
			Context.Logout();

			Target.Process(new BatchForTest
			{
				States = new[]
				{
					new BatchStateForTest
					{
						UserCode = "usercode",
						StateCode = "Телефон"
					}
				}
			});

			Context.Login();
			var actual = UnitOfWork.Get(() => StateGroupRepository.LoadAllCompleteGraph());
			actual.SelectMany(g => g.StateCollection.Select(s => s.StateCode))
				.Should().Contain("Телефон");
		}
	}
}