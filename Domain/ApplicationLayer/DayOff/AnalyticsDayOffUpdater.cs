using System;
using System.Drawing;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.DayOff
{
	[EnabledBy(Toggles.ETL_SpeedUpIntradayDayOff_38213)]
	public class AnalyticsDayOffUpdater :
		IHandleEvent<DayOffTemplateChangedEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsDayOffRepository _analyticsDayOffRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;

		private readonly static ILog logger = LogManager.GetLogger(typeof(AnalyticsDayOffUpdater));

		public AnalyticsDayOffUpdater(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IAnalyticsDayOffRepository analyticsDayOffRepository, IDayOffTemplateRepository dayOffTemplateRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsDayOffRepository = analyticsDayOffRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
			if (logger.IsInfoEnabled)
			{
				logger.Info($"New instance of {nameof(AnalyticsDayOffUpdater)} was created");
			}
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		public virtual void Handle(DayOffTemplateChangedEvent @event)
		{
			if (logger.IsDebugEnabled)
			{
				logger.Debug($"Consuming {nameof(DayOffTemplateChangedEvent)} for day off template id = {@event.DayOffTemplateId}.");
			}

			var businessUnitId = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId).BusinessUnitId;
			var dayOffTemplate = _dayOffTemplateRepository.Get(@event.DayOffTemplateId);
			
			var dayOff = new AnalyticsDayOff
			{
				DayOffCode = @event.DayOffTemplateId,
				BusinessUnitId = businessUnitId,
				DayOffName = dayOffTemplate.Description.Name,
				DayOffShortname = dayOffTemplate.Description.ShortName,
				DisplayColor = dayOffTemplate.DisplayColor.ToArgb(),
				DisplayColorHtml = ColorTranslator.ToHtml(dayOffTemplate.DisplayColor),
				DatasourceUpdateDate = dayOffTemplate.UpdatedOn ?? DateTime.UtcNow,
				DatasourceId = 1
			};
			
			_analyticsDayOffRepository.AddOrUpdate(dayOff);
		}
	}
}
