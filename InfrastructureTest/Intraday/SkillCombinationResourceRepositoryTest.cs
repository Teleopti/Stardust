using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Intraday;

namespace Teleopti.Ccc.InfrastructureTest.Intraday
{
	[TestFixture]
	[UnitOfWorkTest]
	public class SkillCombinationResourceRepositoryTest
	{
		public ISkillCombinationResourceRepository Target;

		[Test]
		public void ShouldPersistSkillCombination()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var combinations = new List<List<Guid>>();
			combinations.Add(new List<Guid> {skill1});
			combinations.Add(new List<Guid> {skill1,skill2});
			Target.PersistSkillCombination(combinations);

			var combinationId1 =  Target.LoadSkillCombination(new List<Guid> { skill1, skill2 });
			var combinationId2 = Target.LoadSkillCombination(new List<Guid> { skill1 });
			combinationId1.Should().Not.Be.EqualTo(combinationId2);
		}

		[Test]
		public void ShouldReturnNullIfCombinationDoesntExists()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var combinations = new List<List<Guid>>();
			combinations.Add(new List<Guid> { skill1 });
			Target.PersistSkillCombination(combinations);

			var combinationId1 = Target.LoadSkillCombination(new List<Guid> { skill1, skill2 });
			combinationId1.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldNotAddIfTheCombinationExists()
		{
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var combinations = new List<List<Guid>>();
			combinations.Add(new List<Guid> { skill1 });
			combinations.Add(new List<Guid> { skill1, skill2 });
			Target.PersistSkillCombination(combinations);
			
			var combinationIdBefore = Target.LoadSkillCombination(new List<Guid> { skill1, skill2 });
			Target.PersistSkillCombination(new List<IEnumerable<Guid>> { new List<Guid> { skill1, skill2 }});

			var combinationIdAfter = Target.LoadSkillCombination(new List<Guid> { skill1, skill2 });
			combinationIdBefore.Should().Be.EqualTo(combinationIdAfter);
		}
	}

	
}
