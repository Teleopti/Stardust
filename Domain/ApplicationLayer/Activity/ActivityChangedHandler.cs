using System.Drawing;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Activity
{
	[EnabledBy(Toggles.ETL_SpeedUpIntradayActivity_38303)]
	public class ActivityChangedHandler :
		IHandleEvent<ActivityChangedEvent>,
		IHandleEvent<ActivityDeleteEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IAnalyticsActivityRepository _analyticsActivityRepository;

		private readonly static ILog logger = LogManager.GetLogger(typeof(ActivityChangedHandler));

		public ActivityChangedHandler(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IActivityRepository activityRepository, IAnalyticsActivityRepository analyticsActivityRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_activityRepository = activityRepository;
			_analyticsActivityRepository = analyticsActivityRepository;
			logger.Debug($"New instance of {nameof(ActivityChangedHandler)} was created");
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(ActivityChangedEvent @event)
		{
			logger.Debug($"Consuming {nameof(ActivityChangedEvent)} for event id = {@event.ActivityId}.");
			var analyticsActivity =
				_analyticsActivityRepository.Activities().FirstOrDefault(a => a.ActivityCode == @event.ActivityId);

			var applicationActivity = _activityRepository.Load(@event.ActivityId);
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);

			// Add
			if (analyticsActivity == null)
			{
				_analyticsActivityRepository.AddActivity(new AnalyticsActivity
				{
					ActivityCode = @event.ActivityId,
					ActivityName = applicationActivity.Name,
					DisplayColor = applicationActivity.DisplayColor.ToArgb(),
					DisplayColorHtml = ColorTranslator.ToHtml(applicationActivity.DisplayColor),
					InReadyTime = applicationActivity.InReadyTime,
					InContractTime = applicationActivity.InContractTime,
					InPaidTime = applicationActivity.InPaidTime,
					InWorkTime = applicationActivity.InWorkTime,
					BusinessUnitId = analyticsBusinessUnit.BusinessUnitId,
					DatasourceId = 1,
					DatasourceUpdateDate = applicationActivity.UpdatedOn.GetValueOrDefault(),
					IsDeleted = applicationActivity.IsDeleted
				});
			}
			else
			// Update
			{
				_analyticsActivityRepository.UpdateActivity(new AnalyticsActivity
				{
					ActivityCode = @event.ActivityId,
					ActivityName = applicationActivity.Name,
					DisplayColor = applicationActivity.DisplayColor.ToArgb(),
					DisplayColorHtml = ColorTranslator.ToHtml(applicationActivity.DisplayColor),
					InReadyTime = applicationActivity.InReadyTime,
					InContractTime = applicationActivity.InContractTime,
					InPaidTime = applicationActivity.InPaidTime,
					InWorkTime = applicationActivity.InWorkTime,
					BusinessUnitId = analyticsActivity.BusinessUnitId,
					DatasourceId = 1,
					DatasourceUpdateDate = applicationActivity.UpdatedOn.GetValueOrDefault(),
					IsDeleted = applicationActivity.IsDeleted
				});
			}
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(ActivityDeleteEvent @event)
		{
			logger.Debug($"Consuming {nameof(ActivityDeleteEvent)} for event id = {@event.ActivityId}.");
			var analyticsActivity = _analyticsActivityRepository.Activities().FirstOrDefault(a => a.ActivityCode == @event.ActivityId);

			if (analyticsActivity == null)
				return;

			_analyticsActivityRepository.UpdateActivity(new AnalyticsActivity
			{
				ActivityCode = @event.ActivityId,
				ActivityName = analyticsActivity.ActivityName,
				DisplayColor = analyticsActivity.DisplayColor,
				DisplayColorHtml = analyticsActivity.DisplayColorHtml,
				InReadyTime = analyticsActivity.InReadyTime,
				InContractTime = analyticsActivity.InContractTime,
				InPaidTime = analyticsActivity.InPaidTime,
				InWorkTime = analyticsActivity.InWorkTime,
				BusinessUnitId = analyticsActivity.BusinessUnitId,
				DatasourceId = analyticsActivity.DatasourceId,
				DatasourceUpdateDate = analyticsActivity.DatasourceUpdateDate,
				IsDeleted = true
			});
		}
	}
}
