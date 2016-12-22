using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Skill;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Skill
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class AnalyticsSkillUpdaterTest
	{
		private AnalyticsSkillUpdater target;
		private FakeAnalyticsSkillRepository analyticsSkillRepository;
		private ISkillRepository skillRepository;
		private FakeAnalyticsTimeZoneRepository analyticsTimeZoneRepository;
		private FakeAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository;

		[SetUp]
		public void SetUp()
		{
			analyticsSkillRepository = new FakeAnalyticsSkillRepository();
			skillRepository = new FakeSkillRepository();
			analyticsTimeZoneRepository = new FakeAnalyticsTimeZoneRepository();
			analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			target = new AnalyticsSkillUpdater(skillRepository, analyticsSkillRepository, analyticsBusinessUnitRepository,
				analyticsTimeZoneRepository);
		}

		[Test]
		public void ShouldAddSkill()
		{
			var @event = new SkillCreatedEvent
			{
				SkillId = Guid.NewGuid()
			};
			var skill = SkillFactory.CreateSkill("skillName1").WithId(@event.SkillId);
			skillRepository.Add(skill);
			target.Handle(@event);

			analyticsSkillRepository.Skills(2)
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
			skillRepository.Add(skill);

			target.Handle(@event);

			analyticsSkillRepository.Skills(2)
				.FirstOrDefault(x => x.SkillCode == skill.Id.GetValueOrDefault())
				.Should()
				.Not.Be.Null();
		}
	}
}