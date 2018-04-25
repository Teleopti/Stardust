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
	public class AnalyticsSkillUpdaterTest : IIsolateSystem, IExtendSystem
	{
		public AnalyticsSkillUpdater Target;
		public FakeAnalyticsSkillRepository AnalyticsSkillRepository;
		public ISkillRepository SkillRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeEventPublisher EventPublisher;
		private Guid _businessUnitId;
		
		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<AnalyticsSkillUpdater>();
		}
		
		public void Isolate(IIsolate isolate)
		{
			_businessUnitId = Guid.NewGuid();
		}
		
		[Test]
		public void ShouldAddSkill()
		{
			var @event = new SkillCreatedEvent
			{
				SkillId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnitId
			};
			var skill = SkillFactory.CreateSkill("skillName1").WithId(@event.SkillId);
			SkillRepository.Add(skill);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
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
				SkillId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnitId
			};
			var skill = SkillFactory.CreateSkill("skillName1").WithId(@event.SkillId);
			SkillRepository.Add(skill);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			Target.Handle(@event);

			AnalyticsSkillRepository.Skills(2)
				.FirstOrDefault(x => x.SkillCode == skill.Id.GetValueOrDefault())
				.Should()
				.Not.Be.Null();
		}
		
		[Test]
		public void ShouldPublishEventForTimeZoneChanges()
		{
			var @event = new SkillCreatedEvent
			{
				SkillId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnitId
			};
			var skill = SkillFactory.CreateSkill("skillName1").WithId(@event.SkillId);
			SkillRepository.Add(skill);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			Target.Handle(@event);

			EventPublisher.PublishedEvents.Count(a => a.GetType() == typeof(PossibleTimeZoneChangeEvent)).Should().Be.EqualTo(1);
		}
	}
}