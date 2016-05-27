using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Skill;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Skill
{
	[TestFixture]
	public class AnalyticsSkillUpdaterTest
	{
		FakeAnalyticsSkillRepository analyticsSkillRepository;
		ISkillRepository skillRepository;
		IAnalyticsTimeZoneRepository analyticsTimeZoneRepository;
		FakeAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository;

		[SetUp]
		public void SetUp()
		{
			analyticsSkillRepository = new FakeAnalyticsSkillRepository();
			skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			analyticsTimeZoneRepository = MockRepository.GenerateMock<IAnalyticsTimeZoneRepository>();
			analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
		}

		[Test]
		public void ShouldAddSkill()
		{
			var @event = new SkillCreatedEvent
			{
				SkillId = Guid.NewGuid()
			};
			var skill = SkillFactory.CreateSkill("skillName1");
			skillRepository.Stub(x => x.Get(@event.SkillId)).Return(skill);
			analyticsTimeZoneRepository.Stub(x => x.Get(skill.TimeZone.Id)).Return(new AnalyticsTimeZone());

			var target = new AnalyticsSkillUpdater(skillRepository, analyticsSkillRepository, analyticsBusinessUnitRepository, analyticsTimeZoneRepository);

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
			var skill = SkillFactory.CreateSkill("skillName1");
			skillRepository.Stub(x => x.Get(@event.SkillId)).Return(skill);
			analyticsTimeZoneRepository.Stub(x => x.Get(skill.TimeZone.Id)).Return(new AnalyticsTimeZone());

			var target = new AnalyticsSkillUpdater(skillRepository, analyticsSkillRepository, analyticsBusinessUnitRepository, analyticsTimeZoneRepository);

			target.Handle(@event);

			analyticsSkillRepository.Skills(2)
				.FirstOrDefault(x => x.SkillCode == skill.Id.GetValueOrDefault())
				.Should()
				.Not.Be.Null();
		}
	}
}