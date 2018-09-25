using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsDateRepositoryWithCreation : IAnalyticsDateRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;
		private readonly IEventPublisher _eventPublisher;
		private readonly IAnalyticsConfigurationRepository _analyticsConfigurationRepository;
		private static bool shouldPublish;

		public AnalyticsDateRepositoryWithCreation(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork,
			IAnalyticsConfigurationRepository analyticsConfigurationRepository,
			IEventPublisher eventPublisher) 
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
			_eventPublisher = eventPublisher;
			_analyticsConfigurationRepository = analyticsConfigurationRepository;
		}
		
		public IAnalyticsDate MaxDate()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Ge(nameof(AnalyticsDate.DateId), 0))
				.AddOrder(Order.Desc(nameof(AnalyticsDate.DateId)))
				.SetMaxResults(1)
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>() ?? new AnalyticsDate {DateDate = new DateTime(1999, 12, 30) };
		}

		public IAnalyticsDate MinDate()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Ge(nameof(AnalyticsDate.DateId), 0))
				.AddOrder(Order.Asc(nameof(AnalyticsDate.DateId)))
				.SetMaxResults(1)
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>() ?? AnalyticsDate.NotDefined;
		}

		private IAnalyticsDate dateInDb(DateTime dateDate)
		{
			if (dateDate == AnalyticsDate.Eternity.DateDate)
				return AnalyticsDate.Eternity;
			return _currentAnalyticsUnitOfWork.Current().Session().CreateCriteria<AnalyticsDate>()
				.Add(Restrictions.Eq(nameof(AnalyticsDate.DateDate), dateDate.Date))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IList<IAnalyticsDate> GetAllPartial()
		{
			AnalyticsDatePartial analyticsDatePartial = null;
			return _currentAnalyticsUnitOfWork.Current().Session().QueryOver<AnalyticsDate>()
				.Where(ad => ad.DateId >= 0)
				.SelectList(list => list
					.Select(d => d.DateId).WithAlias(() => analyticsDatePartial.DateId)
					.Select(d => d.DateDate).WithAlias(() => analyticsDatePartial.DateDate)
					)
				.OrderBy(ad => ad.DateId)
				.Asc
				.TransformUsing(Transformers.AliasToBean<AnalyticsDatePartial>())
				.List<IAnalyticsDate>();
		}

		public IAnalyticsDate Date(DateTime dateDate)
		{
			return dateInDb(dateDate) ?? createDatesTo(dateDate.Date);
		}

		private IAnalyticsDate createDatesTo(DateTime dateDate)
		{
			var toDate = dateDate.AddDays(42);
			var currentDay = MaxDate().DateDate;
			var culture = _analyticsConfigurationRepository.GetCulture();

			var session = _currentAnalyticsUnitOfWork.Current().Session();
			var oneDay = TimeSpan.FromDays(1);
			while ((currentDay += oneDay) <= toDate)
			{
				session.Save(new AnalyticsDate(currentDay, culture));
				shouldPublish = true;
			}

			_currentAnalyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				if (!shouldPublish) return;
				_eventPublisher.Publish(new AnalyticsDatesChangedEvent());
				shouldPublish = false;
			});

			return dateInDb(dateDate);
		}
	}
}