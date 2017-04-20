using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.Intraday
{
	[DomainTest]
	public class FetchSkillAreaTest
	{
		public FetchSkillArea Target;
		public FakeSkillAreaRepository SkillAreaRepository;
		public FakeLoadAllSkillInIntradays LoadAllSkillInIntradays;


		[Test]
		public void ShouldGetAll()
		{
			SkillAreaRepository.Has(new SkillArea { Skills = new List<SkillInIntraday>() }.WithId());
			SkillAreaRepository.Has(new SkillArea { Skills = new List<SkillInIntraday>() }.WithId());

			Target.GetAll().Count()
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMapViewModelCorrectly()
		{
			var existingSkillArea = new SkillArea
			{
				Name = RandomName.Make(),
				Skills = new List<SkillInIntraday>
				{
					new SkillInIntraday { Id = Guid.NewGuid(), Name = RandomName.Make(), IsDeleted = false}
				}
			}.WithId();

			SkillAreaRepository.Has(existingSkillArea);

			var skillInIntraday = new SkillInIntraday() {Id = existingSkillArea.Skills.First().Id, DoDisplayData = true, SkillType = "InboundPhone"};
			LoadAllSkillInIntradays.Has(skillInIntraday);

			var result = Target.GetAll().Single();

			result.Should().Be.OfType<SkillAreaViewModel>();
			result.Id.Should().Be.EqualTo(existingSkillArea.Id);
			result.Name.Should().Be.EqualTo(existingSkillArea.Name);
			result.Skills.Count().Should().Be.EqualTo(1);
			var skill = existingSkillArea.Skills.First();
			var mappedSkill = result.Skills.First();
			mappedSkill.Id.Should().Be.EqualTo(skill.Id);
			mappedSkill.Name.Should().Be.EqualTo(skill.Name);
			mappedSkill.IsDeleted.Should().Be.EqualTo(skill.IsDeleted);
			mappedSkill.SkillType.Should().Be.EqualTo(skillInIntraday.SkillType);
			mappedSkill.DoDisplayData.Should().Be.EqualTo(skillInIntraday.DoDisplayData);
		}

		[Test]
		public void ShouldIncludeDeletedSkillInSkillArea()
		{
			var existingSkillArea = new SkillArea
			{
				Name = RandomName.Make(),
				Skills = new List<SkillInIntraday>
				{
					new SkillInIntraday { Id = Guid.NewGuid(), Name = RandomName.Make(), IsDeleted = true}
				}
			}.WithId();

			SkillAreaRepository.Has(existingSkillArea);

			var result = Target.GetAll().Single();

			result.Should().Be.OfType<SkillAreaViewModel>();
			result.Id.Should().Be.EqualTo(existingSkillArea.Id);
			result.Name.Should().Be.EqualTo(existingSkillArea.Name);
			result.Skills.Count().Should().Be.EqualTo(1);
			var skill = existingSkillArea.Skills.First();
			var mappedSkill = result.Skills.First();
			mappedSkill.Id.Should().Be.EqualTo(skill.Id);
			mappedSkill.Name.Should().Be.EqualTo(skill.Name);
			mappedSkill.IsDeleted.Should().Be.EqualTo(skill.IsDeleted);
			mappedSkill.SkillType.Should().Be.EqualTo(null);
			mappedSkill.DoDisplayData.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldGetSingleViewModel()
		{
			var skillAreaId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var existingSkillArea = new SkillArea
			{
				Name = "SkillAreaName",
				Skills = new List<SkillInIntraday>
				{
					new SkillInIntraday { Id = skillId, Name = "Phone", IsDeleted = false }
				}
			}.WithId(skillAreaId);
			SkillAreaRepository.Has(new SkillArea());
			SkillAreaRepository.Has(existingSkillArea);

			var result = Target.Get(skillAreaId);
			
			result.Name.Should().Be("SkillAreaName");
			result.Skills.Single().Id.Should().Be(skillId);
			result.Skills.Single().Name.Should().Be("Phone");
			result.Skills.Single().IsDeleted.Should().Be(false);
		}
	}
}