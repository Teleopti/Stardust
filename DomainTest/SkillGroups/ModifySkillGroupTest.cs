using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SkillGroups
{
	[DomainTest]
	public class ModifySkillGroupTest
	{
		public ModifySkillGroup Target;
		public FakeSkillGroupRepository SkillGroupRepository;

		[Test]
		public void ShouldUpdate()
		{
			var skillGroup = new SkillGroup().WithId();
			skillGroup.Name = "SkillGroup1";
			var skillToRemove = Guid.NewGuid();

			var skills = new List<SkillInIntraday>
			{
				new SkillInIntraday
				{
					Id = Guid.NewGuid()
				},
				new SkillInIntraday
				{
					Id = Guid.NewGuid()
				},
				new SkillInIntraday
				{
					Id = skillToRemove
				}
			};
			skillGroup.Skills = skills;
			SkillGroupRepository.Has(skillGroup);

			var updateSkills = skills.Take(2).Select(x => new SkillInIntradayViewModel { Id = x.Id }).ToList();
			var newSkill = new SkillInIntradayViewModel { Id = Guid.NewGuid()};
			var newSkill2 = new SkillInIntradayViewModel { Id = Guid.NewGuid() };

			updateSkills.Add(newSkill);
			updateSkills.Add(newSkill2);

			Target.Do(new ModifySkillGroupInput {
				Id = skillGroup.Id.ToString(),
				Name = "SkillGroup2",
				Skills = updateSkills
			});

			var updatedSkillGroup = SkillGroupRepository.Get(skillGroup.Id ?? Guid.Empty);

			updatedSkillGroup.Should().Not.Be.Null();
			updatedSkillGroup.Skills.Count.Should().Be.EqualTo(4);
			updatedSkillGroup.Name.Should().Be.EqualTo("SkillGroup2");
			updatedSkillGroup.Skills.SingleOrDefault(x => x.Id == newSkill.Id).Should().Not.Be.Null();
			updatedSkillGroup.Skills.SingleOrDefault(x => x.Id == skillToRemove).Should().Be.Null();
		}
	}
}
