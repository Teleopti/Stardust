using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[UnitOfWorkTest]
	public class PersonSkillsReadModelPersisterTest
	{
		public WithUnitOfWork UnitOfWork;
		public IPersonSkillsReadModelPersister Target;
		public IAgentStateReadModelReader Reader;
		public IAgentStateReadModelPersister StatePersister;

		[Test]
		public void ShouldPersistModel()
		{
			var person = Guid.NewGuid();
			var skill = Guid.NewGuid();
			StatePersister.Persist(new AgentStateReadModelForTest { PersonId = person });

			Target.Persist(new PersonSkillsReadModel { PersonId = person, SkillIds = new[] { skill } });

			var result = Reader.LoadForSkill(skill).Single();
			result.PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldPersistModelWithServeralSkills()
		{
			var person = Guid.NewGuid();
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			StatePersister.Persist(new AgentStateReadModelForTest { PersonId = person });

			Target.Persist(new PersonSkillsReadModel { PersonId = person, SkillIds = new[] { skill1, skill2 } });

			var result = Reader.LoadForSkill(skill1).Single();
			result.PersonId.Should().Be(person);
			result = Reader.LoadForSkill(skill2).Single();
			result.PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldUpdateModel()
		{
			var person = Guid.NewGuid();
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			StatePersister.Persist(new AgentStateReadModelForTest { PersonId = person });
			Target.Persist(new PersonSkillsReadModel { PersonId = person, SkillIds = new[] { skill1 } });

			Target.Persist(new PersonSkillsReadModel { PersonId = person, SkillIds = new[] { skill2 } });

			Reader.LoadForSkill(skill1).Should().Have.Count.EqualTo(0);
			var result = Reader.LoadForSkill(skill2).Single();
			result.PersonId.Should().Be(person);
		}
		
		[Test]
		public void ShouldDeleteModel()
		{
			var person = Guid.NewGuid();
			var skill = Guid.NewGuid();
			StatePersister.Persist(new AgentStateReadModelForTest { PersonId = person });
			Target.Persist(new PersonSkillsReadModel { PersonId = person, SkillIds = new[] { skill } });

			Target.Delete(person);

			Reader.LoadForSkill(skill).Should().Have.Count.EqualTo(0);
		}
	}
}
