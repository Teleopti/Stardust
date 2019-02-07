using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	[EnabledBy(Toggles.WFM_Forecast_Readmodel_80790)]
	[InstancePerLifetimeScope]
	public class TriggerSkillForecastReadModelHandler : IHandleEvent<TenantHourTickEvent>, IRunOnHangfire
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly SkillForecastReadModelPeriodBuilder _skillForecastReadModelPeriodBuilder;
		private readonly ISkillForecastJobStartTimeRepository _skillForecastJobStartTimeRepository;
		private readonly INow _now;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly SkillForecastSettingsReader _skillForecastSettingsReader;

		public TriggerSkillForecastReadModelHandler(IEventPublisher eventPublisher, SkillForecastReadModelPeriodBuilder skillForecastReadModelPeriodBuilder, ISkillForecastJobStartTimeRepository skillForecastJobStartTimeRepository, INow now, IBusinessUnitRepository businessUnitRepository, SkillForecastSettingsReader skillForecastSettingsReader)
		{
			_eventPublisher = eventPublisher;
			_skillForecastReadModelPeriodBuilder = skillForecastReadModelPeriodBuilder;
			_skillForecastJobStartTimeRepository = skillForecastJobStartTimeRepository;
			_now = now;
			_businessUnitRepository = businessUnitRepository;
			_skillForecastSettingsReader = skillForecastSettingsReader;
		}

		[UnitOfWork]
		public virtual void Handle(TenantHourTickEvent @event)
		{
			var businessUnits = _businessUnitRepository.LoadAll();
			businessUnits.ForEach(businessUnit =>
			{
				var businessUnitId = businessUnit.Id.GetValueOrDefault();
				var lastRun = _skillForecastJobStartTimeRepository.GetLastCalculatedTime(businessUnitId);
				var period = _skillForecastReadModelPeriodBuilder.BuildFullPeriod();
				if (lastRun.HasValue)
				{
					
					var isLockTimeValid = _skillForecastJobStartTimeRepository.IsLockTimeValid(businessUnitId);
					var nextRun = lastRun.Value.AddDays(_skillForecastSettingsReader.NumberOfDaysForNextJobRun);
					if (nextRun <= _now.UtcDateTime() || !isLockTimeValid) 
						period = _skillForecastReadModelPeriodBuilder.BuildNextPeriod(lastRun.GetValueOrDefault());
					else
					{
						return;
					}
				}

				_eventPublisher.Publish(new UpdateSkillForecastReadModelEvent()
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime,
					LogOnBusinessUnitId = businessUnitId,
				});
			});

		}
	}
}