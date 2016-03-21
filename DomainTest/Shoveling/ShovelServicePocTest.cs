using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Shoveling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Shoveling
{
	[TestFixture]
	public class ShovelServicePocTest
	{
		[Test]
		public void ShouldShovelFromNearestPile()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, 1);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, 1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, -1);

			var periodList = new List<ISkillStaffPeriod> {periodA, periodB, periodC};
			target.Execute(periodList, double.MaxValue);

			periodA.AbsoluteDifference.Should().Be.EqualTo(1);
			periodB.AbsoluteDifference.Should().Be.EqualTo(0);
			periodC.AbsoluteDifference.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShovelFromNextNearestPileIfNearestIsNotEnough()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, 1);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, 1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, -1.5);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC };
			target.Execute(periodList, double.MaxValue);

			periodA.AbsoluteDifference.Should().Be.EqualTo(0.5);
			periodB.AbsoluteDifference.Should().Be.EqualTo(0);
			periodC.AbsoluteDifference.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldTryToCoverMostExpensivePotFirst()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, 1);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, -1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, -1.5);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC };
			target.Execute(periodList, double.MaxValue);

			periodA.AbsoluteDifference.Should().Be.EqualTo(0);
			periodB.AbsoluteDifference.Should().Be.EqualTo(0);
			periodC.AbsoluteDifference.Should().Be.EqualTo(-1.5);
		}

		[Test]
		public void ShouldLeaveMoreOfExpensivePileIfMoreThanOnePile()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, 2);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, -1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, 1);
			var skillD = SkillFactory.CreateSkill("levelD");
			var periodD = createSkillStaffPeriod(skillD, -1.5);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC, periodD };
			target.Execute(periodList, double.MaxValue);

			periodA.AbsoluteDifference.Should().Be.EqualTo(0.5);
			periodB.AbsoluteDifference.Should().Be.EqualTo(0);
			periodC.AbsoluteDifference.Should().Be.EqualTo(0);
			periodD.AbsoluteDifference.Should().Be.EqualTo(0);
		}

		[Test]
		public void MayNotTransferMoreThanAllovedForSkillGroup()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, 2);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, -1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, 1);
			var skillD = SkillFactory.CreateSkill("levelD");
			var periodD = createSkillStaffPeriod(skillD, -1.5);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC, periodD };
			target.Execute(periodList, 2);

			periodA.AbsoluteDifference.Should().Be.EqualTo(1);
			periodB.AbsoluteDifference.Should().Be.EqualTo(0);
			periodC.AbsoluteDifference.Should().Be.EqualTo(0);
			periodD.AbsoluteDifference.Should().Be.EqualTo(-0.5);
		}

		[Test]
		public void ShouldHandleEverythingUnderStaffed()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, -1);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, -1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, -1);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC };
			target.Execute(periodList, 2);

			periodA.AbsoluteDifference.Should().Be.EqualTo(-1);
			periodB.AbsoluteDifference.Should().Be.EqualTo(-1);
			periodC.AbsoluteDifference.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldHandleEverythingOverStaffed()
		{
			var target = new Shovel();
			var skillA = SkillFactory.CreateSkill("levelA");
			var periodA = createSkillStaffPeriod(skillA, 1);
			var skillB = SkillFactory.CreateSkill("levelB");
			var periodB = createSkillStaffPeriod(skillB, 1);
			var skillC = SkillFactory.CreateSkill("levelC");
			var periodC = createSkillStaffPeriod(skillC, 1);

			var periodList = new List<ISkillStaffPeriod> { periodA, periodB, periodC };
			target.Execute(periodList, 2);

			periodA.AbsoluteDifference.Should().Be.EqualTo(1);
			periodB.AbsoluteDifference.Should().Be.EqualTo(1);
			periodC.AbsoluteDifference.Should().Be.EqualTo(1);
		}

		private ISkillStaffPeriod createSkillStaffPeriod(ISkill skill, double absoluteDifference)
		{
			var forecasted = Math.Abs(absoluteDifference);
			var resource = forecasted + absoluteDifference;
			var period = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, new DateTime(2016, 3, 19, 12, 0, 0, DateTimeKind.Utc), forecasted, 0);
			period.SetCalculatedResource65(resource);

			return period;
		}

	}

	[TestFixture]
	public class SkillGroupPriotizerTest
	{
		[Test]
		public void HiLevelSkillgroupsShouldBeAtTheBottomAndLowerLevelsHigherUp()
		{
			var skillA = SkillFactory.CreateSkill("levelA");
			var skillB = SkillFactory.CreateSkill("levelB");
			var skillC = SkillFactory.CreateSkill("levelC");

			var g1 = new AffectedSkills { Skills = new List<ISkill> { skillA, skillC } }; //2 
			var g2 = new AffectedSkills { Skills = new List<ISkill> { skillB, skillC } }; //1
			var g3 = new AffectedSkills { Skills = new List<ISkill> { skillA, skillB, skillC } }; //4
			var g4 = new AffectedSkills { Skills = new List<ISkill> { skillA, skillB } }; //3

			var unorderedSkillGroupList = new List<AffectedSkills>{ g1, g2, g3, g4 };

			var result = new SkillGroupPriotizer().Sort(unorderedSkillGroupList, new List<ISkill> { skillA, skillB, skillC });
			result[0].Skills.ToList().Should().Contain(skillB).And.Contain(skillC);		
			result[1].Skills.ToList().Should().Contain(skillA).And.Contain(skillC);
			result[2].Skills.ToList().Should().Contain(skillA).And.Contain(skillB);
			result[3].Skills.ToList().Should().Contain(skillA).And.Contain(skillB).And.Contain(skillC);
		}

		[Test]
		public void ShouldHandle64SkillsWithFastAlgoritm()
		{
			var skillList = new List<ISkill>();
			for (int i = 0; i < 64; i++)
			{
				skillList.Add(SkillFactory.CreateSkill(i.ToString()));
			}

			var skillGroup = new AffectedSkills { Skills = skillList };
			var result = new SkillGroupPriotizer().Sort(new List<AffectedSkills>{ skillGroup }, skillList);
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleMoreThan64SkillsWithSlowAlgoritm()
		{
			var skillList = new List<ISkill>();
			var skillGroupSkills1 = new List<ISkill>();
			var skillGroupSkills2 = new List<ISkill>();
			var skill0 = SkillFactory.CreateSkill("0");
			var skill99 = SkillFactory.CreateSkill("99");
			for (int i = 0; i < 100; i++)
			{
				var skillToAdd = SkillFactory.CreateSkill(i.ToString());
				if(i == 0)
					skillToAdd = skill0;
				else if(i == 99)
					skillToAdd = skill99;

				skillList.Add(skillToAdd);
				if(i < 25)
					skillGroupSkills1.Add(skillToAdd);
				else
					skillGroupSkills2.Add(skillToAdd);
			}

			var skillGroup1 = new AffectedSkills { Skills = skillGroupSkills1 };
			var skillGroup2 = new AffectedSkills { Skills = skillGroupSkills2 };
			var result = new SkillGroupPriotizer().Sort(new List<AffectedSkills> { skillGroup1, skillGroup2 }, skillList);
			result[0].Skills.Should().Contain(skill99);
			result[1].Skills.Should().Contain(skill0);
		}
	}
}