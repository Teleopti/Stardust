using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.SkillGroup;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.SkillGroups
{
	[DomainTest]
	public class FetchSkillGroupTest : ISetup
	{
		public FetchSkillGroup Target;
		public FakeSkillGroupRepository SkillGroupRepository;
		public FakeLoadAllSkillInIntradays LoadAllSkillInIntradays;
		public FakeUserUiCulture UiCulture;
		private readonly List<SkillInIntraday> defaultSkills = new List<SkillInIntraday>();

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
			defaultSkills.Add(new SkillInIntraday
			{
				Id = Guid.NewGuid(),
				Name = RandomName.Make()
			});
			defaultSkills.Add(new SkillInIntraday
			{
				Id = Guid.NewGuid(),
				Name = RandomName.Make()
			});
		}		

		[Test]
		public void ShouldGetAll()
		{
			SkillGroupRepository.Has(new SkillGroup { Skills = defaultSkills }.WithId());
			SkillGroupRepository.Has(new SkillGroup { Skills = defaultSkills }.WithId());

			Target.GetAll().Count()
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSortNameInSkillarea()
		{
			SkillGroupRepository.Has(new SkillGroup { Name = "B", Skills = defaultSkills }.WithId());
			SkillGroupRepository.Has(new SkillGroup { Name = "C", Skills = defaultSkills }.WithId());
			SkillGroupRepository.Has(new SkillGroup { Name = "A", Skills = defaultSkills }.WithId());

			UiCulture.IsSwedish();
			Target.GetAll().Select(sa => sa.Name).Should().Have.SameSequenceAs(new[] { "A", "B", "C" });
		}

		[Test]
		public void ShouldSortSwedishNameInSkillarea()
		{
			SkillGroupRepository.Has(new SkillGroup { Name = "Ä", Skills = defaultSkills }.WithId());
			SkillGroupRepository.Has(new SkillGroup { Name = "A", Skills = defaultSkills }.WithId());
			SkillGroupRepository.Has(new SkillGroup { Name = "Å", Skills = defaultSkills }.WithId());

			UiCulture.IsSwedish();
			Target.GetAll().Select(sa => sa.Name).Should().Have.SameSequenceAs(new[] { "A", "Å", "Ä" });
		}

		[Test]
		public void ShouldMapViewModelCorrectly()
		{
			var existingSkillArea = new SkillGroup
			{
				Name = RandomName.Make(),
				Skills = new List<SkillInIntraday>
				{
					new SkillInIntraday { Id = Guid.NewGuid(), Name = RandomName.Make(), IsDeleted = false }
				}
			}.WithId();

			SkillGroupRepository.Has(existingSkillArea);

			var skillInIntraday = new SkillInIntraday() { Id = existingSkillArea.Skills.First().Id, DoDisplayData = true, SkillType = "InboundPhone" };
			LoadAllSkillInIntradays.Has(skillInIntraday);

			var result = Target.GetAll().Single();

			result.Should().Be.OfType<SkillGroupViewModel>();
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
		public void ShouldNotIncludeDeletedSkillInSkillArea()
		{
			var existingSkillArea = new SkillGroup
			{
				Name = RandomName.Make(),
				Skills = new List<SkillInIntraday>
				{
					new SkillInIntraday { Id = Guid.NewGuid(), Name = RandomName.Make(), IsDeleted = false },
					new SkillInIntraday { Id = Guid.NewGuid(), Name = RandomName.Make(), IsDeleted = true }
				}
			}.WithId();

			SkillGroupRepository.Has(existingSkillArea);

			var result = Target.GetAll().Single();

			result.Should().Be.OfType<SkillGroupViewModel>();
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
			var existingSkillArea = new SkillGroup
			{
				Name = "SkillAreaName",
				Skills = new List<SkillInIntraday>
				{
					new SkillInIntraday { Id = skillId, Name = "Phone", IsDeleted = false }
				}
			}.WithId(skillAreaId);
			SkillGroupRepository.Has(new SkillGroup());
			SkillGroupRepository.Has(existingSkillArea);

			var result = Target.Get(skillAreaId);

			result.Name.Should().Be("SkillAreaName");
			result.Skills.Single().Id.Should().Be(skillId);
			result.Skills.Single().Name.Should().Be("Phone");
			result.Skills.Single().IsDeleted.Should().Be(false);
		}

		[Test]
		public void ShouldNotGetSkillAreaWithOnlyDeletedSkills()
		{
			var existingSkillArea = new SkillGroup
			{
				Name = RandomName.Make(),
				Skills = new List<SkillInIntraday>
				{
					new SkillInIntraday { Id = Guid.NewGuid(), Name = RandomName.Make(), IsDeleted = true },
					new SkillInIntraday { Id = Guid.NewGuid(), Name = RandomName.Make(), IsDeleted = true }
				}
			}.WithId();

			SkillGroupRepository.Has(existingSkillArea);

			var result = Target.GetAll().FirstOrDefault();

			result.Should().Be.Null();
		}
	}
}