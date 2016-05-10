using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.WinCode.Forecasting.Cascading;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.Cascading
{
	[DomainTest]
	public class CascadingSkillPresenterTest
	{
		public FakeSkillRepository SkillRepository;

		[Test]
		public void ShouldPlaceNonCascadingSkillInNonCascadingList()
		{
			var skill = new Skill();
			SkillRepository.Add(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.NonCascadingSkills.Should().Have.SameSequenceAs(skill);
			target.CascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void ShouldPlaceCascadingSkillInCascadingList()
		{
			var skill = new Skill();
			skill.SetCascadingIndex_UseFromTestOnly(1);
			SkillRepository.Add(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill);
			target.NonCascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void MakeSkillCascading()
		{
			var skill = new Skill();
			SkillRepository.Add(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeCascading(skill);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill);
			target.NonCascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void DoNotMakeSkillCascadingWhenAlreadyIsCascading()
		{
			var skill = new Skill();
			skill.SetCascadingIndex_UseFromTestOnly(14);
			SkillRepository.Add(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeCascading(skill);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill);
			target.NonCascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void MakeSkillNonCascading()
		{
			var skill = new Skill();
			skill.SetCascadingIndex_UseFromTestOnly(11);
			SkillRepository.Add(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeNonCascading(skill);

			target.NonCascadingSkills.Should().Have.SameSequenceAs(skill);
			target.CascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void DoNotMakeNonSkillCascadingWhenAlreadyIsNonCascading()
		{
			var skill = new Skill();
			SkillRepository.Add(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeNonCascading(skill);

			target.NonCascadingSkills.Should().Have.SameSequenceAs(skill);
			target.CascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void ShouldSetIndexOnCascadingSkills()
		{
			var skill1 = new Skill();
			var skill2 = new Skill();
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);
			target.MakeCascading(skill1);
			target.MakeCascading(skill2);
			target.Confirm();

			skill1.CascadingIndex.Should().Be.EqualTo(1);
			skill2.CascadingIndex.Should().Be.EqualTo(2);
			skill1.IsCascading().Should().Be.True();
			skill2.IsCascading().Should().Be.True();
		}

		[Test]
		public void ShouldClearIndexOnCascadingSkills()
		{
			var skill1 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(13);
			var skill2 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(57);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);
			target.MakeNonCascading(skill1);
			target.MakeNonCascading(skill2);

			target.Confirm();

			skill1.IsCascading().Should().Be.False();
			skill2.IsCascading().Should().Be.False();
		}

		[Test]
		public void ShouldMoveUpCascadingSkill()
		{
			var skill3 = new Skill();
			skill3.SetCascadingIndex_UseFromTestOnly(3);
			var skill2 = new Skill();
			skill2.SetCascadingIndex_UseFromTestOnly(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(1);
			SkillRepository.Add(skill3);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveUpCascadingSkill(skill3);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill1, skill3, skill2);
		}

		[Test]
		public void ShouldNotMoveUpCascadingSkillIfFirstInList()
		{
			var skill2 = new Skill();
			skill2.SetCascadingIndex_UseFromTestOnly(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(1);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveUpCascadingSkill(skill1);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill1, skill2);
		}

		[Test]
		public void ShouldNotMoveUpIfNotCascadingSkill()
		{
			var skill2 = new Skill();
			var skill1 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(1);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveUpCascadingSkill(skill2);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill1);
		}

		[Test]
		public void ShouldMoveDownCascadingSkill()
		{
			var skill3 = new Skill();
			skill3.SetCascadingIndex_UseFromTestOnly(3);
			var skill2 = new Skill();
			skill2.SetCascadingIndex_UseFromTestOnly(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(1);
			SkillRepository.Add(skill3);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveDownCascadingSkill(skill1);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill2, skill1, skill3);
		}

		[Test]
		public void ShouldNotMoveDownCascadingSkillIfLastInList()
		{
			var skill2 = new Skill();
			skill2.SetCascadingIndex_UseFromTestOnly(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(1);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveDownCascadingSkill(skill2);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill1, skill2);
		}

		[Test]
		public void ShouldNotMoveDownIfNotCascadingSkill()
		{
			var skill2 = new Skill();
			var skill1 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(1);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveDownCascadingSkill(skill2);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill1);
		}

		[Test]
		public void ShouldOrderCascadingSkillListAtStart()
		{
			var skill3 = new Skill();
			skill3.SetCascadingIndex_UseFromTestOnly(3);
			var skill2 = new Skill();
			skill2.SetCascadingIndex_UseFromTestOnly(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex_UseFromTestOnly(1);
			SkillRepository.Add(skill3);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.CascadingSkills.Should().Have.SameSequenceAs(skill1, skill2, skill3);
		}


		[Test]
		public void ShouldOrderNonCascadingSkillListAtStart()
		{
			var skill3 = new Skill();
			skill3.ChangeName("3");
			var skill2 = new Skill();
			skill2.ChangeName("2");
			var skill1 = new Skill();
			skill1.ChangeName("1");
			SkillRepository.Add(skill3);
			SkillRepository.Add(skill1);
			SkillRepository.Add(skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.NonCascadingSkills.Should().Have.SameSequenceAs(skill1, skill2, skill3);
		}
	}
}