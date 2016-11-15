using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Cascading
{
	[DomainTest]
	public class SkillRoutingPriorityPersisterTest
	{
		public SkillRoutingPriorityPersister Target;
		public FakeSkillRepository SkillRepository;

		[Test]
		public void ShouldConfirmCascadingIndexes()
		{
			var activity = new Activity("activity").WithId();
			var skillRoutingPriorityRows = new List<SkillRoutingPriorityModelRow>();
			skillRoutingPriorityRows.Add(createRow(createSkill("skill1", activity), null));
			skillRoutingPriorityRows.Add(createRow(createSkill("skill2", activity), 7));
			skillRoutingPriorityRows.Add(createRow(createSkill("skill3", activity), 2));
			skillRoutingPriorityRows.Add(createRow(createSkill("skill4", activity), 7));
			Target.Persist(skillRoutingPriorityRows);

			var skillList = SkillRepository.LoadAll();
			var skillDic = skillList.ToDictionary(s => s.Name);
			skillDic["skill1"].CascadingIndex.HasValue.Should().Be.False();
			skillDic["skill2"].CascadingIndex.Value.Should().Be.EqualTo(1);
			skillDic["skill3"].CascadingIndex.Value.Should().Be.EqualTo(2);
			skillDic["skill4"].CascadingIndex.Value.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotThrowIfSkillsWithDifferentActivityHaveSamePrioValue()
		{
			var activity1 = new Activity("activity1").WithId();
			var activity2 = new Activity("activity2").WithId();
			var skillRoutingPriorityRows = new List<SkillRoutingPriorityModelRow>();
			skillRoutingPriorityRows.Add(createRow(createSkill("skill1", activity1), 7));
			skillRoutingPriorityRows.Add(createRow(createSkill("skill2", activity2), 7));
			Assert.DoesNotThrow(() => Target.Persist(skillRoutingPriorityRows));
		}

		[Test]
		public void ShouldNotTrowIfSkillsWithSameActivityDoesNotHaveAnyPrio()
		{
			var activity1 = new Activity("activity1").WithId();
			var activity2 = new Activity("activity2").WithId();
			var skillRoutingPriorityRows = new List<SkillRoutingPriorityModelRow>();
			skillRoutingPriorityRows.Add(createRow(createSkill("skill1", activity1), null));
			skillRoutingPriorityRows.Add(createRow(createSkill("skill2", activity2), null));
			Assert.DoesNotThrow(() => Target.Persist(skillRoutingPriorityRows));
		}

		private SkillRoutingPriorityModelRow createRow(ISkill skill, int? priority)
		{
			return new SkillRoutingPriorityModelRow
			{
				SkillGuid = skill.Id.Value,
				SkillName = skill.Name,
				ActivityGuid = skill.Activity.Id.Value,
				ActivityName = skill.Activity.Name,
				Priority = priority	
			};
		}

		private ISkill createSkill(string skillName, IActivity activity)
		{
			var skill = SkillFactory.CreateSkillWithId(skillName);
			skill.Activity = activity;
			SkillRepository.Add(skill);
			return skill;
		}
	}
}