using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Skill;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Skill
{
	[DomainTest]
	public class AnalyticsSkillUpdaterTest : ISetup
	{
		public AnalyticsSkillUpdater Target;
		public FakeAnalyticsSkillRepository AnalyticsSkillRepository;
		public ISkillRepository SkillRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsSkillUpdater>();
		}

		[Test]
		public void ShouldAddSkill()
		{
			var @event = new SkillCreatedEvent
			{
				SkillId = Guid.NewGuid()
			};
			var skill = SkillFactory.CreateSkill("skillName1").WithId(@event.SkillId);
			SkillRepository.Add(skill);
			Target.Handle(@event);

			AnalyticsSkillRepository.Skills(2)
				.FirstOrDefault(x => x.SkillCode == skill.Id.GetValueOrDefault())
				.Should()
				.Not.Be.Null();
		}

		[Test]
		public void ShouldAddOrUpdateSkill()
		{
			var @event = new SkillChangedEvent
			{
				SkillId = Guid.NewGuid()
			};
			var skill = SkillFactory.CreateSkill("skillName1").WithId(@event.SkillId);
			SkillRepository.Add(skill);

			Target.Handle(@event);

			AnalyticsSkillRepository.Skills(2)
				.FirstOrDefault(x => x.SkillCode == skill.Id.GetValueOrDefault())
				.Should()
				.Not.Be.Null();
		}
	}
}