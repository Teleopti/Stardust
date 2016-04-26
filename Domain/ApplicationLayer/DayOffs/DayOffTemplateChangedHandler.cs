using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Preference;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.DayOffs
{
	[UseOnToggle(Toggles.ETL_SpeedUpIntradayDayOff_38213)]
	public class DayOffTemplateChangedHandler :
		IHandleEvent<DayOffTemplateChangedEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsDayOffRepository _analyticsDayOffRepository;

		private readonly static ILog logger = LogManager.GetLogger(typeof(PreferenceChangedHandler));

		public DayOffTemplateChangedHandler(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IAnalyticsDayOffRepository analyticsDayOffRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsDayOffRepository = analyticsDayOffRepository;
			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		public virtual void Handle(DayOffTemplateChangedEvent @event)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consuming event for day off template id = {0}. (Message timestamp = {1})",
								   @event.DayOffTemplateId, @event.Timestamp);
			}

			var businessUnitId = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId).BusinessUnitId;
			var dayOff = new AnalyticsDayOff
			{
				DayOffCode = @event.DayOffTemplateId,
				BusinessUnitId = businessUnitId,
				DayOffName = @event.DayOffName,
				DayOffShortname = @event.DayOffShortName,
				DisplayColor = -8355712,
				DisplayColorHtml = "#808080",
				DatasourceUpdateDate = @event.DatasourceUpdateDate
			};
			
			_analyticsDayOffRepository.AddOrUpdate(dayOff);
		}
	}
}
