using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class RuleSetSkillActivityCheckerTest
	{
		private IRuleSetSkillActivityChecker _target;
		private IList<ISkill> _skills;
		private IWorkShiftRuleSet _workShiftRuleSet;
		private Activity _act1;
		private Activity _act2;
		private Activity _act3;


		[SetUp]
		public void Setup()
		{
			_target = new RuleSetSkillActivityChecker();
			_act1 = ActivityFactory.CreateActivity("act1");
			_act1.RequiresSkill = true;
			_act2 = ActivityFactory.CreateActivity("act2");
			_act2.RequiresSkill = true;
			_act3 = ActivityFactory.CreateActivity("act3");
			_act3.RequiresSkill = false;
			_workShiftRuleSet = WorkShiftRuleSetFactory.Create();
			_workShiftRuleSet.TemplateGenerator.BaseActivity = _act1;
			_skills = new List<ISkill>();
		}

		[Test]
		public void ShouldReturnFalseIfBaseActivityIsSkillActivityAndNotInSkillList()
		{
			var skill = SkillFactory.CreateSkill("hej");
			skill.Activity = _act2;
			_skills.Add(skill);
			bool result = _target.CheckSkillActivities(_workShiftRuleSet, _skills);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfAnyOfTheExtendersHaveSkillActivityAndNotInList()
		{
			var extender = new AutoPositionedActivityExtender(_act2, new TimePeriodWithSegment(), TimeSpan.FromMinutes(1));
			_workShiftRuleSet.AddExtender(extender);
			var skill = SkillFactory.CreateSkill("hej");
			skill.Activity = _act1;
			_skills.Add(skill);
			bool result = _target.CheckSkillActivities(_workShiftRuleSet, _skills);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnTrueIfAllSkillActivitiesInRuleSetIsFoundInTheList()
		{
			var extender = new AutoPositionedActivityExtender(_act2, new TimePeriodWithSegment(), TimeSpan.FromMinutes(1));
			_workShiftRuleSet.AddExtender(extender);
			var skill1 = SkillFactory.CreateSkill("hej");
			skill1.Activity = _act1;
			_skills.Add(skill1);
			var skill2 = SkillFactory.CreateSkill("hej");
			skill2.Activity = _act2;
			_skills.Add(skill2);
			bool result = _target.CheckSkillActivities(_workShiftRuleSet, _skills);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldIgnoreNoneSkillActivities()
		{
			_workShiftRuleSet.TemplateGenerator.BaseActivity = _act3;
			var extender = new AutoPositionedActivityExtender(_act3, new TimePeriodWithSegment(), TimeSpan.FromMinutes(1));
			_workShiftRuleSet.AddExtender(extender);
			bool result = _target.CheckSkillActivities(_workShiftRuleSet, _skills);
			Assert.IsTrue(result);
		}

	}
}