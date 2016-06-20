using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[DatabaseTest]
	[TestFixture]
	public class AgentStateReadModelReaderForSkillTest
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldLoad()
		{
			Database
				.WithAgent()
				.WithSkill("phone");
			var personId = Database.CurrentPersonId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.Persist(new AgentStateReadModelForTest { PersonId = personId });
			});

			WithUnitOfWork.Get(() => Target.LoadBySkill(currentSkillId))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldOnlyLoadAgentsForSelectedSkill()
		{
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("email");
			var agent1 = Database.PersonIdFor("agent1");
			var agent2 = Database.PersonIdFor("agent2");
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {agent1, agent2});
				StatePersister.Persist(new AgentStateReadModelForTest {PersonId = agent1});
				StatePersister.Persist(new AgentStateReadModelForTest {PersonId = agent2});
			});

			WithUnitOfWork.Get(() => Target.LoadBySkill(currentSkillId))
				.Single().PersonId.Should().Be(agent1);
		}

		[Test]
		public void ShouldNotLoadForPreviousSkill()
		{
			Now.Is("2016-06-20");
			Database
				.WithPerson("agent1")
				.WithPersonPeriod("2016-01-01".Date())
				.WithSkill("email")
				.WithPersonPeriod("2016-06-19".Date())
				.WithSkill("phone")
				;
			var personId = Database.CurrentPersonId();
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.Persist(new AgentStateReadModelForTest { PersonId = personId });
			});

			WithUnitOfWork.Get(() => Target.LoadBySkill(email))
				.Should().Be.Empty();
		}
	}
}