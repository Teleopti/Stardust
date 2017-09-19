using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class ModifySkillGroupTest
	{
		public ModifySkillGroup Target;
		public FakeSkillAreaRepository SkillAreaRepository;

		[Test]
		public void ShouldUpdate()
		{
			var skillGroup = new SkillArea().WithId();
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
			SkillAreaRepository.Has(skillGroup);

			var updateSkills = skills.Take(2).Select(x => x.Id).ToList();
			Guid newSkillId = Guid.NewGuid();

			updateSkills.Add(newSkillId);
			updateSkills.Add(Guid.NewGuid());

			Target.Do(new ModifySkillGroupInput {
				Id = skillGroup.Id.Value,
				Name = "SkillGroup2",
				Skills = updateSkills
			});

			var updatedSkillGroup = SkillAreaRepository.Get(skillGroup.Id.Value);

			updatedSkillGroup.Should().Not.Be.Null();
			updatedSkillGroup.Skills.Count.Should().Be.EqualTo(4);
			updatedSkillGroup.Name.Should().Be.EqualTo("SkillGroup2");
			updatedSkillGroup.Skills.SingleOrDefault(x => x.Id == newSkillId).Should().Not.Be.Null();
			updatedSkillGroup.Skills.SingleOrDefault(x => x.Id == skillToRemove).Should().Be.Null();
		}
	}
}
