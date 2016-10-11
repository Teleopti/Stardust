using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Cascading
{
	[DomainTest]
	public class SkillRoutingPriorityModelTest
	{
		public SkillRoutingPriorityModel Target;
		public FakeSkillRepository SkillRepository;

		[Test]
		public void ShouldReturnSkillData()
		{
			var activity = new Activity("_").WithId();
			SkillRepository.Has("Skill1", activity, null);
			SkillRepository.Has("Skill2", activity, 1);

			var result = Target.SkillRoutingPriorityModelRows();
			result.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMapSkillWithCascadingIndex()
		{
			var activity = new Activity("activity").WithId();
			var skill = SkillFactory.CreateSkillWithId("_");
			skill.Activity = activity;
			skill.SetCascadingIndex(3);
			SkillRepository.Add(skill);

			var result = Target.SkillRoutingPriorityModelRows();
			result.Count.Should().Be.EqualTo(1);
			result[0].SkillGuid.Should().Be.EqualTo(skill.Id);
			result[0].ActivityGuid.Should().Be.EqualTo(activity.Id);
			result[0].ActivityName.Should().Be.EqualTo(activity.Name);
			result[0].SkillName.Should().Be.EqualTo(skill.Name);
			result[0].Priority.HasValue.Should().Be.True();
			result[0].Priority.Value.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldMapSkillWithOutCascadingIndex()
		{
			var activity = new Activity("activity").WithId();
			var skill = SkillFactory.CreateSkillWithId("_");
			skill.Activity = activity;
			SkillRepository.Add(skill);

			var result = Target.SkillRoutingPriorityModelRows();
			result.Count.Should().Be.EqualTo(1);
			result[0].SkillGuid.Should().Be.EqualTo(skill.Id);
			result[0].ActivityGuid.Should().Be.EqualTo(activity.Id);
			result[0].ActivityName.Should().Be.EqualTo(activity.Name);
			result[0].SkillName.Should().Be.EqualTo(skill.Name);
			result[0].Priority.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldNotReturnDeletedSkillData()
		{
			var activity = new Activity("activity").WithId();
			var skill = SkillFactory.CreateSkillWithId("_");
			skill.Activity = activity;
			var deletedSkill = SkillFactory.CreateSkillWithId("_");
			((IDeleteTag)deletedSkill).SetDeleted();
			SkillRepository.Add(skill);
			SkillRepository.Add(deletedSkill);

			var result = Target.SkillRoutingPriorityModelRows();
			result.Count.Should().Be.EqualTo(1);
			result[0].SkillGuid.Should().Be.EqualTo(skill.Id);
			result[0].ActivityGuid.Should().Be.EqualTo(activity.Id);
			result[0].ActivityName.Should().Be.EqualTo(activity.Name);
			result[0].SkillName.Should().Be.EqualTo(skill.Name);
			result[0].Priority.Should().Be.EqualTo(null);
		}
	}
}