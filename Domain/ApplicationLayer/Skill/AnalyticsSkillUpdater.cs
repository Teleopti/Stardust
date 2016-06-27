using System;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Skill
{
	[EnabledBy(Toggles.ETL_SpeedUpIntradaySkill_37543)]
	public class AnalyticsSkillUpdater :
		IHandleEvent<SkillChangedEvent>,
		IHandleEvent<SkillCreatedEvent>,
		IHandleEvent<SkillDeletedEvent>,
		IRunOnHangfire
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IAnalyticsSkillRepository _analyticsSkillRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsSkillUpdater));

		public AnalyticsSkillUpdater(ISkillRepository skillRepository, IAnalyticsSkillRepository analyticsSkillRepository, IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IAnalyticsTimeZoneRepository analyticsTimeZoneRepository)
		{
			_skillRepository = skillRepository;
			_analyticsSkillRepository = analyticsSkillRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;
		}
		
		public virtual void Handle(Guid skillId)
		{
			var skill = _skillRepository.Get(skillId);
			if (skill == null)
			{
				logger.Warn($"Skill {skillId} does not exists in application.");
				return;
			}
			var businessUnit = _analyticsBusinessUnitRepository.Get(skill.BusinessUnit.Id.GetValueOrDefault());
			var timezone = _analyticsTimeZoneRepository.Get(skill.TimeZone.Id);
			if (businessUnit == null) throw new BusinessUnitMissingInAnalyticsException();
			if (timezone == null) throw new TimeZoneMissingInAnalyticsException();
			
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
		public virtual void Handle(SkillChangedEvent @event)
		{
			Handle(@event.SkillId);
		}

		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(SkillCreatedEvent @event)
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