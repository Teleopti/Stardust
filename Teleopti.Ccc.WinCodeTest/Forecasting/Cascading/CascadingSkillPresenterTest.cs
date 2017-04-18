using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.Cascading;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

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
			SkillRepository.Has(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.NonCascadingSkills.Should().Have.SameSequenceAs(skill);
			target.CascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceMaxSeatSkillInAnyList()
		{
			var skill = new Skill {SkillType = new SkillTypePhone(new Description("_"), ForecastSource.MaxSeatSkill)};
			SkillRepository.Has(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.NonCascadingSkills.Should().Be.Empty();
			target.CascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPlaceMaxSeatSkillWithOrderIndexInAnyList()
		{
			var skill = new Skill { SkillType = new SkillTypePhone(new Description("_"), ForecastSource.MaxSeatSkill) };
			skill.SetCascadingIndex(1);
			SkillRepository.Has(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.NonCascadingSkills.Should().Be.Empty();
			target.CascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void ShouldPlaceCascadingSkillInCascadingList()
		{
			var skill = new Skill();
			skill.SetCascadingIndex(1);
			SkillRepository.Has(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill);
			target.NonCascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void MakeSkillCascading()
		{
			var skill = new Skill();
			SkillRepository.Has(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeCascading(skill);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill);
			target.NonCascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void DoNotMakeSkillCascadingWhenAlreadyIsCascading()
		{
			var skill = new Skill();
			skill.SetCascadingIndex(14);
			SkillRepository.Has(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeCascading(skill);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill);
			target.NonCascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void MakeSkillNonCascading()
		{
			var skill = new Skill();
			skill.SetCascadingIndex(11);
			SkillRepository.Has(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeNonCascading(skill);

			target.NonCascadingSkills.Should().Have.SameSequenceAs(skill);
			target.CascadingSkills.Should().Be.Empty();
		}

		[Test]
		public void DoNotMakeNonSkillCascadingWhenAlreadyIsNonCascading()
		{
			var skill = new Skill();
			SkillRepository.Has(skill);
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
			SkillRepository.Has(skill1, skill2);
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
			skill1.SetCascadingIndex(13);
			var skill2 = new Skill();
			skill1.SetCascadingIndex(57);
			SkillRepository.Has(skill1, skill2);
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
			skill3.SetCascadingIndex(3);
			var skill2 = new Skill();
			skill2.SetCascadingIndex(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex(1);
			SkillRepository.Has(skill3, skill1, skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveUpCascadingSkills(skill3);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill1);
			target.CascadingSkills[1].Should().Have.SameSequenceAs(skill3);
			target.CascadingSkills[2].Should().Have.SameSequenceAs(skill2);
		}

		[Test]
		public void ShouldNotMoveUpCascadingSkillIfFirstInList()
		{
			var skill2 = new Skill();
			skill2.SetCascadingIndex(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex(1);
			SkillRepository.Has(skill1, skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveUpCascadingSkills(skill1);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill1);
			target.CascadingSkills[1].Should().Have.SameSequenceAs(skill2);
		}

		[Test]
		public void ShouldNotMoveUpIfNotCascadingSkill()
		{
			var skill2 = new Skill();
			var skill1 = new Skill();
			skill1.SetCascadingIndex(1);
			SkillRepository.Has(skill1, skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveUpCascadingSkills(skill2);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill1);
		}

		[Test]
		public void ShouldMoveDownCascadingSkill()
		{
			var skill3 = new Skill();
			skill3.SetCascadingIndex(3);
			var skill2 = new Skill();
			skill2.SetCascadingIndex(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex(1);
			SkillRepository.Has(skill3, skill1, skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveDownCascadingSkills(skill1);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill2);
			target.CascadingSkills[1].Should().Have.SameSequenceAs(skill1);
			target.CascadingSkills[2].Should().Have.SameSequenceAs(skill3);
		}

		[Test]
		public void ShouldNotMoveDownCascadingSkillIfLastInList()
		{
			var skill2 = new Skill();
			skill2.SetCascadingIndex(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex(1);
			SkillRepository.Has(skill1, skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveDownCascadingSkills(skill2);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill1);
			target.CascadingSkills[1].Should().Have.SameSequenceAs(skill2);
		}

		[Test]
		public void ShouldNotMoveDownIfNotCascadingSkill()
		{
			var skill2 = new Skill();
			var skill1 = new Skill();
			skill1.SetCascadingIndex(1);
			SkillRepository.Has(skill1, skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MoveDownCascadingSkills(skill2);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill1);
		}

		[Test]
		public void ShouldOrderCascadingSkillListAtStart()
		{
			var skill3 = new Skill();
			skill3.SetCascadingIndex(3);
			var skill2 = new Skill();
			skill2.SetCascadingIndex(2);
			var skill1 = new Skill();
			skill1.SetCascadingIndex(1);
			SkillRepository.Has(skill3, skill1, skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skill1);
			target.CascadingSkills[1].Should().Have.SameSequenceAs(skill2);
			target.CascadingSkills[2].Should().Have.SameSequenceAs(skill3);
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
			SkillRepository.Has(skill3, skill1, skill2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.NonCascadingSkills.Should().Have.SameSequenceAs(skill1, skill2, skill3);
		}

		[Test]
		public void ShouldOrderMixOfSkills()
		{
			var nonCascading1 = new Skill();
			nonCascading1.ChangeName("1");
			var nonCascading2 = new Skill();
			nonCascading2.ChangeName("2");
			var cascading1 = new Skill();
			cascading1.SetCascadingIndex(1);
			cascading1.ChangeName("3");
			var cascading2 = new Skill();
			cascading2.SetCascadingIndex(2);
			cascading2.ChangeName("0");
			SkillRepository.Has(cascading2, nonCascading2, nonCascading1, cascading1);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.NonCascadingSkills.Should().Have.SameSequenceAs(nonCascading1, nonCascading2);
			target.CascadingSkills[0].Should().Have.SameSequenceAs(cascading1);
			target.CascadingSkills[1].Should().Have.SameSequenceAs(cascading2);
		}

		[Test]
		public void ShouldNotLoadDeletedSkills()
		{
			var skill = new Skill();
			skill.SetDeleted();
			SkillRepository.Has(skill);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.NonCascadingSkills.Should().Be.Empty();	
		}

		[Test]
		public void ShouldMakeTwoSkillsParalell()
		{
			var skillA=new Skill();
			skillA.SetCascadingIndex(1);
			var skillB = new Skill();
			skillB.SetCascadingIndex(2);
			SkillRepository.Has(skillA, skillB);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeParalell(skillA, skillB);

			target.CascadingSkills.Single().Should().Have.SameValuesAs(skillA, skillB);
		}

		[Test]
		public void ShouldIncludeAlreadyParalellSkillWhenMakeParalell()
		{
			var skillA = new Skill();
			skillA.SetCascadingIndex(1);
			var skillB1 = new Skill();
			skillB1.SetCascadingIndex(2);
			var skillB2 = new Skill();
			skillB2.SetCascadingIndex(2);
			SkillRepository.Has(skillA, skillB1, skillB2);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeParalell(skillA, skillB1);

			target.CascadingSkills.Single().Should().Have.SameValuesAs(skillA, skillB1, skillB2);
		}

		[Test]
		public void ShouldPlaceParalellSkillsOnPositionOfFirstParameter()
		{
			var skillA = new Skill();
			skillA.SetCascadingIndex(1);
			var skillB = new Skill();
			skillB.SetCascadingIndex(2);
			var skillC = new Skill();
			skillC.SetCascadingIndex(3);
			SkillRepository.Has(skillA, skillB, skillC);

			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeParalell(skillC, skillA);

			target.CascadingSkills[0].Should().Have.SameValuesAs(skillB);
			target.CascadingSkills[1].Should().Have.SameValuesAs(skillC, skillA);
		}

		[Test]
		public void ShouldOrderParalellSkillsByName()
		{
			var skillA = new Skill();
			skillA.ChangeName("A");
			skillA.SetCascadingIndex(1);
			var skillB = new Skill();
			skillB.ChangeName("B");
			skillB.SetCascadingIndex(2);
			SkillRepository.Has(skillA, skillB);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.MakeParalell(skillB, skillA);

			target.CascadingSkills[0].Should().Have.SameSequenceAs(skillA, skillB);
		}

		[Test]
		public void ShouldUnparalell()
		{
			var skillA = new Skill();
			skillA.SetCascadingIndex(1);
			var skillB1 = new Skill();
			skillB1.SetCascadingIndex(2);
			var skillB2 = new Skill();
			skillB2.SetCascadingIndex(2);
			var skillC = new Skill();
			skillC.SetCascadingIndex(3);
			SkillRepository.Has(skillA, skillB1, skillB2, skillC);
			var target = new CascadingSkillPresenter(SkillRepository);

			target.Unparalell(skillB1);

			target.CascadingSkills[0].Should().Have.SameValuesAs(skillA);
			target.CascadingSkills[1].Should().Have.SameValuesAs(skillB1);
			target.CascadingSkills[2].Should().Have.SameValuesAs(skillB2);
			target.CascadingSkills[3].Should().Have.SameValuesAs(skillC);
		}
	}
}