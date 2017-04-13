using System;
using System.Drawing;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Activity
{
	public class AnalyticsActivityUpdater :
		IHandleEvent<ActivityChangedEvent>,
		IRunOnHangfire
	{
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IAnalyticsActivityRepository _analyticsActivityRepository;

		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsActivityUpdater));

		public AnalyticsActivityUpdater(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository, IActivityRepository activityRepository, IAnalyticsActivityRepository analyticsActivityRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_activityRepository = activityRepository;
			_analyticsActivityRepository = analyticsActivityRepository;
			logger.Debug($"New instance of {nameof(AnalyticsActivityUpdater)} was created");
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ActivityChangedEvent @event)
		{
			logger.Debug($"Consuming {nameof(ActivityChangedEvent)} for event id = {@event.ActivityId}.");
			if (@event.ActivityId == Guid.Empty)
				return;
			var analyticsActivity =
				_analyticsActivityRepository.Activities().FirstOrDefault(a => a.ActivityCode == @event.ActivityId);

			var applicationActivity = _activityRepository.Get(@event.ActivityId);
			if (applicationActivity == null)
			{
				logger.Warn($"{nameof(IActivity)} '{@event.ActivityId}' was not found in application database.");
				return;
			}
			var analyticsBusinessUnit = _analyticsBusinessUnitRepository.Get(@event.LogOnBusinessUnitId);
			if (analyticsBusinessUnit == null) throw new BusinessUnitMissingInAnalyticsException();

			var activity = new AnalyticsActivity
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
			};

			// Add
			if (analyticsActivity == null)
			{
				_analyticsActivityRepository.AddActivity(activity);
			}
			else
			// Update
			{
				_analyticsActivityRepository.UpdateActivity(activity);
			}
		}
	}
}
