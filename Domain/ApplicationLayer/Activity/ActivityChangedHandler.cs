using System;
using log4net;
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
		private readonly IBusinessUnitRepository _businessUnitRepository;

		private readonly static ILog logger = LogManager.GetLogger(typeof(ActivityChangedHandler));

		public ActivityChangedHandler(IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,IBusinessUnitRepository businessUnitRepository)
		{
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_businessUnitRepository = businessUnitRepository;

			logger.Debug($"New instance of {nameof(ActivityChangedHandler)} was created");
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(ActivityChangedEvent @event)
		{
			throw new NotImplementedException();
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(ActivityDeleteEvent @event)
		{
			throw new NotImplementedException();
		}
	}
}
