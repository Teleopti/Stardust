using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	[EnabledBy(Toggles.WFM_Forecast_Readmodel_80790)]
	[InstancePerLifetimeScope]
	public class TriggerSkillForecastReadModel : IHandleEvent<TenantDayTickEvent>, IRunOnHangfire
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly SkillForecastReadModelPeriodBuilder _skillForecastReadModelPeriodBuilder;
		private readonly IReadModelStartTimeRepository _readModelStartTimeRepository;
		private readonly INow _now;
		private readonly IBusinessUnitRepository _businessUnitRepository;

		public TriggerSkillForecastReadModel(IEventPublisher eventPublisher, SkillForecastReadModelPeriodBuilder skillForecastReadModelPeriodBuilder, IReadModelStartTimeRepository readModelStartTimeRepository, INow now, IBusinessUnitRepository businessUnitRepository)
		{
			_eventPublisher = eventPublisher;
			_skillForecastReadModelPeriodBuilder = skillForecastReadModelPeriodBuilder;
			_readModelStartTimeRepository = readModelStartTimeRepository;
			_now = now;
			_businessUnitRepository = businessUnitRepository;
		}

		[UnitOfWork]
		public virtual void Handle(TenantDayTickEvent @event)
		{
			var businessUnits = _businessUnitRepository.LoadAll();
			businessUnits.ForEach(businessUnit =>
			{
				var businessUnitId = businessUnit.Id.GetValueOrDefault();
				var lastRun = _readModelStartTimeRepository.GetLastCalculatedTime(businessUnitId,"TriggerSkillForecastReadModel");
				var period = _skillForecastReadModelPeriodBuilder.BuildFullPeriod();
				if (lastRun.HasValue)
				{
					if (lastRun > _now.UtcDateTime().AddDays(-7)) return;
					period = _skillForecastReadModelPeriodBuilder.BuildNextPeriod(lastRun.GetValueOrDefault());
				}

				_eventPublisher.Publish(new UpdateSkillForecastReadModelEvent()
				{
					StartDateTime = period.StartDateTime,
					EndDateTime = period.EndDateTime
				});
			});

		}
	}

	public interface IReadModelStartTimeRepository
	{
		DateTime? GetLastCalculatedTime(Guid bu, string jobName);
	}
}