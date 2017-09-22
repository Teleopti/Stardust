using System;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepositoryWithCreation : AnalyticsDateRepositoryBase, IAnalyticsDateRepository
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IAnalyticsConfigurationRepository _analyticsConfigurationRepository;
		private static bool shouldPublish;

		public AnalyticsDateRepositoryWithCreation(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork, IEventPublisher eventPublisher, IAnalyticsConfigurationRepository analyticsConfigurationRepository) : base(analyticsUnitOfWork)
		{
			_eventPublisher = eventPublisher;
			_analyticsConfigurationRepository = analyticsConfigurationRepository;
		}

		public override IAnalyticsDate Date(DateTime dateDate)
		{
			return base.Date(dateDate) ?? createDatesTo(dateDate.Date);
		}

		private IAnalyticsDate createDatesTo(DateTime dateDate)
		{
			var toDate = dateDate.AddDays(42);
			var currentDay = base.MaxDate().DateDate;
			var culture = _analyticsConfigurationRepository.GetCulture();
			while ((currentDay += TimeSpan.FromDays(1)) <= toDate)
			{
				AnalyticsUnitOfWork.Current().Session().Save(new AnalyticsDate(currentDay, culture));
				shouldPublish = true;
			}
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