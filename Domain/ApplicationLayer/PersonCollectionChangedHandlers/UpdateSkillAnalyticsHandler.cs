using System;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpIntradaySkill_37543)]
	public class UpdateSkillAnalyticsHandler :
		IHandleEvent<SkillNameChangedEvent>,
		IHandleEvent<SkillDeletedEvent>,
		IRunOnHangfire
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IAnalyticsSkillRepository _analyticsSkillRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;

		public UpdateSkillAnalyticsHandler(ISkillRepository skillRepository, IAnalyticsSkillRepository analyticsSkillRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IAnalyticsTimeZoneRepository analyticsTimeZoneRepository)
		{
			_skillRepository = skillRepository;
			_analyticsSkillRepository = analyticsSkillRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;
		}

		
		public virtual void Handle(Guid skillId)
		{
			var skill = _skillRepository.Get(skillId);
			var businessUnit = _analyticsBusinessUnitRepository.Get(skill.BusinessUnit.Id.GetValueOrDefault());
			var timezone = _analyticsTimeZoneRepository.Get(skill.TimeZone.Id);
			if (businessUnit == null) return;
			if (timezone == null) return;
			var deleteTag = skill as IDeleteTag;
			var analyticsSkill = new AnalyticsSkill
			{
				BusinessUnitId = businessUnit.BusinessUnitId,
				DatasourceUpdateDate = skill.UpdatedOn.GetValueOrDefault(),
				SkillName = skill.Name,
				TimeZoneId = timezone.TimeZoneId,
				SkillCode = skill.Id.GetValueOrDefault(),
				ForecastMethodCode = skill.SkillType.Id.GetValueOrDefault(),
				ForecastMethodName = skill.SkillType.ForecastSource.ToString(),
				IsDeleted = deleteTag != null && deleteTag.IsDeleted
			};
			_analyticsSkillRepository.AddOrUpdateSkill(analyticsSkill);
		}

		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(SkillNameChangedEvent @event)
		{
			Handle(@event.SkillId);
		}

		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(SkillDeletedEvent @event)
		{
			Handle(@event.SkillId);
		}
	}
}