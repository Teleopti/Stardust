using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
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
		private IList<IPersonSkill> _personSkills;
		private IWorkShiftRuleSet _workShiftRuleSet;
		private IActivity _act1;
		private IActivity _act2;
		private IActivity _act3;


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
			_personSkills = new List<IPersonSkill>();
		}

		[Test]
		public void ShouldReturnFalseIfBaseActivityIsSkillActivityAndNotInSkillList()
		{
			var skill = SkillFactory.CreateSkill("hej");
			skill.Activity = _act2;
			_personSkills.Add(new PersonSkill(skill,new Percent()));
			bool result = _target.CheckSkillActivties(_workShiftRuleSet, _personSkills);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfAnyOfTheExtendersHaveSkillActivityAndNotInList()
		{
			var extender = new AutoPositionedActivityExtender(_act2, new TimePeriodWithSegment(), TimeSpan.FromMinutes(1));
			_workShiftRuleSet.AddExtender(extender);
			var skill = SkillFactory.CreateSkill("hej");
			skill.Activity = _act1;
			_personSkills.Add(new PersonSkill(skill, new Percent()));
			bool result = _target.CheckSkillActivties(_workShiftRuleSet, _personSkills);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnTrueIfAllSkillActivitiesInRuleSetIsFoundInTheList()
		{
			var extender = new AutoPositionedActivityExtender(_act2, new TimePeriodWithSegment(), TimeSpan.FromMinutes(1));
			_workShiftRuleSet.AddExtender(extender);
			var skill1 = SkillFactory.CreateSkill("hej");
			skill1.Activity = _act1;
			_personSkills.Add(new PersonSkill(skill1, new Percent()));
			var skill2 = SkillFactory.CreateSkill("hej");
			skill2.Activity = _act2;
			_personSkills.Add(new PersonSkill(skill2, new Percent()));
			bool result = _target.CheckSkillActivties(_workShiftRuleSet, _personSkills);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldIgnoreNoneSkillActivities()
		{
			_workShiftRuleSet.TemplateGenerator.BaseActivity = _act3;
			var extender = new AutoPositionedActivityExtender(_act3, new TimePeriodWithSegment(), TimeSpan.FromMinutes(1));
			_workShiftRuleSet.AddExtender(extender);
			bool result = _target.CheckSkillActivties(_workShiftRuleSet, _personSkills);
			Assert.IsTrue(result);
		}

	}
}