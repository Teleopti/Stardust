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
		public FakeAnalyticsTimeZoneRepository AnalyticsTimeZoneRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		private Guid _businessUnitId;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsSkillUpdater>();
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
		public void ShouldUpdateUtcInUse()
		{
			var @event = new SkillCreatedEvent
			{
				SkillId = Guid.NewGuid(),
				LogOnBusinessUnitId = _businessUnitId
			};
			var skill = SkillFactory.CreateSkill("skillName1", TimeZoneInfo.Utc).WithId(@event.SkillId);
			SkillRepository.Add(skill);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("UTC"));

			Target.Handle(@event);

			var utcTimeZone = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(x => x.TimeZoneCode == "UTC");
			utcTimeZone.IsUtcInUse.Should().Be.True();
		}

		[Test]
		public void ShouldMarkTimeZonesAsDeleted()
		{
			const string timeZoneTobeDeleted = "W. Europe Standard Time";
			const string timeZoneInUse = "GTB Standard Time";
			
			var skill = SkillFactory.CreateSkill("skillName1", TimeZoneInfo.FindSystemTimeZoneById(timeZoneTobeDeleted)).WithId();
			SkillRepository.Add(skill);

			AnalyticsTimeZoneRepository.Get(timeZoneInUse);
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInUse));
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			var @event = new SkillDeletedEvent()
			{
				SkillId = skill.Id.Value,
				LogOnBusinessUnitId = _businessUnitId
			};
			Target.Handle(@event);

			var estTimeZone = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == timeZoneTobeDeleted);
			var gtbTimeZone = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == timeZoneInUse);
			var utcTimeZone = AnalyticsTimeZoneRepository.GetAll().FirstOrDefault(timeZone => timeZone.TimeZoneCode == "UTC");
			estTimeZone.ToBeDeleted.Should().Be.True();
			gtbTimeZone.ToBeDeleted.Should().Be.False();
			utcTimeZone.ToBeDeleted.Should().Be.False();
		}

		[Test]
		public void ShouldMarkTimeZoneAsNotDeleted()
		{
			const string timeZoneInUse = "W. Europe Standard Time";
			const string timeZoneDeleted = "GTB Standard Time";
			AnalyticsTimeZoneRepository.Get(timeZoneDeleted);
			AnalyticsTimeZoneRepository.SetToBeDeleted(timeZoneDeleted, true);

			var deleteTedTimeZone = AnalyticsTimeZoneRepository.Get(timeZoneDeleted);
			deleteTedTimeZone.ToBeDeleted.Should().Be.True();

			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneInUse));
			BusinessUnitRepository.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById(timeZoneDeleted));
			var skill = SkillFactory.CreateSkill("skillName1", TimeZoneInfo.FindSystemTimeZoneById(timeZoneDeleted)).WithId();
			SkillRepository.Add(skill);
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(_businessUnitId));

			var @event = new SkillDeletedEvent()
			{
				SkillId = skill.Id.Value,
				LogOnBusinessUnitId = _businessUnitId
			};
			Target.Handle(@event);

			var readdedTimeZone = AnalyticsTimeZoneRepository.Get(timeZoneDeleted);
			readdedTimeZone.ToBeDeleted.Should().Be.False();
		}
	}
}