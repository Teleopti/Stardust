using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class UpdateSkillAnalyticsHandler :
		IHandleEvent<SkillChangedEvent>,
		IRunOnServiceBus
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

		public void Handle(SkillChangedEvent @event)
		{
			var skill = _skillRepository.Get(@event.SkillId);
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
	}
}