using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepositoryWithCreation : AnalyticsDateRepositoryBase, IAnalyticsDateRepository
	{
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;
		private readonly IEventPublisher _eventPublisher;
		private static bool shouldPublish;

		public AnalyticsDateRepositoryWithCreation(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork, IDistributedLockAcquirer distributedLockAcquirer, IEventPublisher eventPublisher) : base(analyticsUnitOfWork)
		{
			_distributedLockAcquirer = distributedLockAcquirer;
			_eventPublisher = eventPublisher;
		}

		public new IAnalyticsDate MaxDate()
		{
			return AnalyticsDate.Eternity;
		}

		public new IAnalyticsDate Date(DateTime dateDate)
		{
			return base.Date(dateDate) ?? createDatesTo(dateDate.Date);
		}

		private IAnalyticsDate createDatesTo(DateTime dateDate)
		{
			using (_distributedLockAcquirer.LockForTypeOf(this))
			{
				// Check again to see that we still don't have the date after aquiring the lock
				var date = base.Date(dateDate);
				if (date != null)
					return date;

				var currentDay = base.MaxDate()?.DateDate ?? new DateTime(1999, 12, 30);
				
				while ((currentDay += TimeSpan.FromDays(1)) <= dateDate)
				{
					AnalyticsUnitOfWork.Current().Session().Save(new AnalyticsDate(currentDay, CultureInfo.CurrentCulture)); // TODO: Should we use some other culture?
					shouldPublish = true;
				}
				//AnalyticsUnitOfWork.Current().PersistAll();
				AnalyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
				{
					if (!shouldPublish) return;
					_eventPublisher.Publish(new AnalyticsDatesChangedEvent());
					shouldPublish = false;
				});
				return base.Date(dateDate);
			}
		}
	}
}